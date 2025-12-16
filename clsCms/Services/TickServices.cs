using clsCms.Interfaces;
using clsCms.Models;
using clsCMs.Data;
using Microsoft.EntityFrameworkCore;

namespace clsCms.Services
{
    /// <summary>
    /// Handles time-based aggregation of article impression analytics.
    ///
    /// This service is designed to be executed by background jobs
    /// (e.g. Azure Functions, WebJobs, or cron-based workers).
    ///
    /// Aggregation levels:
    /// - Raw impressions   → Hourly
    /// - Hourly aggregates → Daily
    /// - Daily aggregates  → Monthly
    ///
    /// Each aggregation step also applies data-retention cleanup
    /// to keep raw tables compact and query-efficient.
    /// </summary>
    public class TickServices : ITickServices
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes the <see cref="TickServices"/>.
        /// </summary>
        public TickServices(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Hourly aggregation

        /// <summary>
        /// Aggregates raw article impressions into hourly performance data.
        ///
        /// - Aggregates impressions for the current UTC hour
        /// - Stores results in the hourly performance table
        /// - Deletes raw impressions older than 7 days
        ///
        /// Intended to run once per hour.
        /// </summary>
        public async Task AggregateHourlyImpressionsAsync()
        {
            // -------------------------------------------------
            // Define aggregation window (UTC hour)
            // -------------------------------------------------
            var now = DateTime.UtcNow;
            var startOfHour = new DateTime(
                now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
            var endOfHour = startOfHour.AddHours(1);

            // -------------------------------------------------
            // Fetch raw impressions in the window
            // -------------------------------------------------
            var rawImpressions = await _dbContext.ArticleImpressions
                .Where(i =>
                    i.ImpressionTime >= startOfHour &&
                    i.ImpressionTime < endOfHour)
                .ToListAsync();

            if (!rawImpressions.Any())
                return;

            // -------------------------------------------------
            // Aggregate raw impressions
            // -------------------------------------------------
            var hourlyAggregates = rawImpressions
                .GroupBy(i => new
                {
                    i.OrganizationId,
                    i.ArticleId,
                    i.ChannelId,
                    Tick = startOfHour
                })
                .Select(g => new ArticleImpressionHourlyModel
                {
                    Tick = g.Key.Tick,
                    OrganizationId = g.Key.OrganizationId,
                    ArticleId = g.Key.ArticleId,
                    ChannelId = g.Key.ChannelId,

                    // Core metrics
                    TotalImpressions = g.Count(),
                    UniqueUsers = g
                        .Select(x => x.UserId)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .Count(),

                    // NOTE:
                    // This is a placeholder approximation.
                    // Replace with real duration tracking if needed.
                    AverageImpressionDuration = g.Average(x => x.ImpressionTime.Second),

                    // Popular dimensions
                    TopReferrer = g
                        .GroupBy(x => x.Referrer)
                        .OrderByDescending(x => x.Count())
                        .Select(x => x.Key)
                        .FirstOrDefault(),

                    TopCountry = g
                        .GroupBy(x => x.Country)
                        .OrderByDescending(x => x.Count())
                        .Select(x => x.Key)
                        .FirstOrDefault(),

                    TopCity = g
                        .GroupBy(x => x.City)
                        .OrderByDescending(x => x.Count())
                        .Select(x => x.Key)
                        .FirstOrDefault()
                })
                .ToList();

            // -------------------------------------------------
            // Persist hourly aggregates
            // -------------------------------------------------
            await _dbContext.ArticleImpressionHourlyPerformance
                .AddRangeAsync(hourlyAggregates);

            await _dbContext.SaveChangesAsync();

            // -------------------------------------------------
            // Cleanup: raw impressions (retention = 7 days)
            // -------------------------------------------------
            var retentionThreshold = now.AddDays(-7);

            var expiredRawImpressions = _dbContext.ArticleImpressions
                .Where(i => i.ImpressionTime < retentionThreshold);

            _dbContext.ArticleImpressions.RemoveRange(expiredRawImpressions);
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Daily aggregation

        /// <summary>
        /// Aggregates hourly impression data into daily performance data.
        ///
        /// - Aggregates yesterday's hourly data
        /// - Stores results in the daily performance table
        /// - Deletes hourly data older than 14 days
        ///
        /// Intended to run once per day.
        /// </summary>
        public async Task AggregateDailyImpressionsAsync()
        {
            var now = DateTime.UtcNow;

            var startOfDay = new DateTime(
                now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc)
                .AddDays(-1);

            var endOfDay = startOfDay.AddDays(1);

            // -------------------------------------------------
            // Fetch hourly aggregates
            // -------------------------------------------------
            var hourlyData = await _dbContext.ArticleImpressionHourlyPerformance
                .Where(i =>
                    i.Tick >= startOfDay &&
                    i.Tick < endOfDay)
                .ToListAsync();

            if (!hourlyData.Any())
                return;

            // -------------------------------------------------
            // Aggregate into daily records
            // -------------------------------------------------
            var dailyAggregates = hourlyData
                .GroupBy(i => new
                {
                    i.OrganizationId,
                    i.ArticleId,
                    i.ChannelId,
                    Tick = startOfDay
                })
                .Select(g => new ArticleImpressionDailyModel
                {
                    Tick = g.Key.Tick,
                    OrganizationId = g.Key.OrganizationId,
                    ArticleId = g.Key.ArticleId,
                    ChannelId = g.Key.ChannelId,

                    TotalImpressions = g.Sum(x => x.TotalImpressions),

                    // NOTE:
                    // Unique users here are an approximation based on hourly aggregates.
                    UniqueUsers = g
                        .Select(x => x.UniqueUsers)
                        .Distinct()
                        .Count(),

                    // Weighted average duration
                    AverageImpressionDuration =
                        g.Sum(x => x.AverageImpressionDuration * x.TotalImpressions)
                        / Math.Max(1, g.Sum(x => x.TotalImpressions)),

                    TopReferrer = g
                        .GroupBy(x => x.TopReferrer)
                        .OrderByDescending(x => x.Sum(y => y.TotalImpressions))
                        .Select(x => x.Key)
                        .FirstOrDefault(),

                    TopCountry = g
                        .GroupBy(x => x.TopCountry)
                        .OrderByDescending(x => x.Sum(y => y.TotalImpressions))
                        .Select(x => x.Key)
                        .FirstOrDefault(),

                    TopCity = g
                        .GroupBy(x => x.TopCity)
                        .OrderByDescending(x => x.Sum(y => y.TotalImpressions))
                        .Select(x => x.Key)
                        .FirstOrDefault()
                })
                .ToList();

            await _dbContext.ArticleImpressionDailyPerformance
                .AddRangeAsync(dailyAggregates);

            await _dbContext.SaveChangesAsync();

            // -------------------------------------------------
            // Cleanup: hourly data (retention = 14 days)
            // -------------------------------------------------
            var hourlyRetentionThreshold = now.AddDays(-14);
            const int batchSize = 1000;

            while (true)
            {
                var expired = await _dbContext
                    .ArticleImpressionHourlyPerformance
                    .Where(i => i.Tick < hourlyRetentionThreshold)
                    .Take(batchSize)
                    .ToListAsync();

                if (!expired.Any())
                    break;

                _dbContext.ArticleImpressionHourlyPerformance.RemoveRange(expired);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion

        #region Monthly aggregation

        /// <summary>
        /// Aggregates daily impression data into monthly performance data.
        ///
        /// - Aggregates previous month's daily data
        /// - Stores results in the monthly performance table
        /// - Deletes daily data older than 1 year
        ///
        /// Intended to run once per month.
        /// </summary>
        public async Task AggregateMonthlyImpressionsAsync()
        {
            var now = DateTime.UtcNow;

            var startOfMonth = new DateTime(
                now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(-1);

            var endOfMonth = startOfMonth.AddMonths(1);

            // -------------------------------------------------
            // Fetch daily aggregates
            // -------------------------------------------------
            var dailyData = await _dbContext.ArticleImpressionDailyPerformance
                .Where(i =>
                    i.Tick >= startOfMonth &&
                    i.Tick < endOfMonth)
                .ToListAsync();

            if (!dailyData.Any())
                return;

            // -------------------------------------------------
            // Aggregate into monthly records
            // -------------------------------------------------
            var monthlyAggregates = dailyData
                .GroupBy(i => new
                {
                    i.OrganizationId,
                    i.ArticleId,
                    i.ChannelId,
                    Tick = startOfMonth
                })
                .Select(g => new ArticleImpressionMonthlyModel
                {
                    Tick = g.Key.Tick,
                    OrganizationId = g.Key.OrganizationId,
                    ArticleId = g.Key.ArticleId,
                    ChannelId = g.Key.ChannelId,

                    TotalImpressions = g.Sum(x => x.TotalImpressions),

                    UniqueUsers = g
                        .Select(x => x.UniqueUsers)
                        .Distinct()
                        .Count(),

                    AverageImpressionDuration =
                        g.Sum(x => x.AverageImpressionDuration * x.TotalImpressions)
                        / Math.Max(1, g.Sum(x => x.TotalImpressions)),

                    TopReferrer = g
                        .GroupBy(x => x.TopReferrer)
                        .OrderByDescending(x => x.Sum(y => y.TotalImpressions))
                        .Select(x => x.Key)
                        .FirstOrDefault(),

                    TopCountry = g
                        .GroupBy(x => x.TopCountry)
                        .OrderByDescending(x => x.Sum(y => y.TotalImpressions))
                        .Select(x => x.Key)
                        .FirstOrDefault(),

                    TopCity = g
                        .GroupBy(x => x.TopCity)
                        .OrderByDescending(x => x.Sum(y => y.TotalImpressions))
                        .Select(x => x.Key)
                        .FirstOrDefault()
                })
                .ToList();

            await _dbContext.ArticleImpressionMonthlyPerformance
                .AddRangeAsync(monthlyAggregates);

            await _dbContext.SaveChangesAsync();

            // -------------------------------------------------
            // Cleanup: daily data (retention = 1 year)
            // -------------------------------------------------
            var dailyRetentionThreshold = now.AddYears(-1);
            const int batchSize = 1000;

            while (true)
            {
                var expired = await _dbContext
                    .ArticleImpressionDailyPerformance
                    .Where(i => i.Tick < dailyRetentionThreshold)
                    .Take(batchSize)
                    .ToListAsync();

                if (!expired.Any())
                    break;

                _dbContext.ArticleImpressionDailyPerformance.RemoveRange(expired);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion
    }
}
