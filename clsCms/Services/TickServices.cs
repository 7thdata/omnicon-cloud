using clsCms.Interfaces;
using clsCms.Models;
using clsCMs.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Services
{
    public class TickServices : ITickServices
    {
        private readonly ApplicationDbContext _dbContext;

        public TickServices(ApplicationDbContext dbContext)
        {

            _dbContext = dbContext;
        }

        /// <summary>
        /// Aggregate Hourly Impression Data.
        /// </summary>
        /// <returns></returns>
        public async Task AggregateHourlyImpressionsAsync()
        {
            // Define the time window for aggregation (last hour)
            var now = DateTime.UtcNow;
            var startOfHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
            var endOfHour = startOfHour.AddHours(1);

            // Query raw impressions within the time window
            var rawImpressions = await _dbContext.ArticleImpressions
                .Where(impression => impression.ImpressionTime >= startOfHour && impression.ImpressionTime < endOfHour)
                .ToListAsync();

            // Group raw impressions by OrganizationId, ArticleId, ChannelId, and Tick
            var groupedImpressions = rawImpressions
                .GroupBy(impression => new
                {
                    impression.OrganizationId,
                    impression.ArticleId,
                    impression.ChannelId,
                    Tick = startOfHour
                })
                .Select(group => new ArticleImpressionHourlyModel
                {
                    Tick = group.Key.Tick,
                    OrganizationId = group.Key.OrganizationId,
                    ArticleId = group.Key.ArticleId,
                    ChannelId = group.Key.ChannelId,
                    TotalImpressions = group.Count(), // Total impressions
                    UniqueUsers = group.Select(g => g.UserId).Distinct().Count(), // Unique users
                    AverageImpressionDuration = group.Average(g => g.ImpressionTime.Second), // Optional
                    TopReferrer = group
                        .GroupBy(g => g.Referrer)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    TopCountry = group
                        .GroupBy(g => g.Country)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    TopCity = group
                        .GroupBy(g => g.City)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault()
                })
                .ToList();

            // Insert aggregated data into the hourly impressions table
            if (groupedImpressions.Any())
            {
                await _dbContext.ArticleImpressionHourlyPerformance.AddRangeAsync(groupedImpressions);
                await _dbContext.SaveChangesAsync();
            }

            Console.WriteLine($"Aggregated {groupedImpressions.Count} hourly impressions for the hour starting {startOfHour}.");

            // Cleanup: Delete raw impressions older than 7 days
            var retentionThreshold = now.AddDays(-7);
            var oldImpressions = _dbContext.ArticleImpressions
                .Where(impression => impression.ImpressionTime < retentionThreshold);

            _dbContext.ArticleImpressions.RemoveRange(oldImpressions);

            var deletedCount = await _dbContext.SaveChangesAsync();
            Console.WriteLine($"Deleted {deletedCount} raw impressions older than {retentionThreshold}.");
        }

        /// <summary>
        /// Aggregate Daily Impression Data from Hourly Data and Clean Up Old Hourly Data.
        /// </summary>
        /// <returns></returns>
        public async Task AggregateDailyImpressionsAsync()
        {
            // Define the time window for aggregation (yesterday)
            var now = DateTime.UtcNow;
            var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc).AddDays(-1);
            var endOfDay = startOfDay.AddDays(1);

            // Query hourly impressions within the time window
            var hourlyImpressions = await _dbContext.ArticleImpressionHourlyPerformance
                .Where(impression => impression.Tick >= startOfDay && impression.Tick < endOfDay)
                .ToListAsync();

            // Group hourly data into daily aggregates
            var dailyImpressions = hourlyImpressions
                .GroupBy(impression => new
                {
                    impression.OrganizationId,
                    impression.ArticleId,
                    impression.ChannelId,
                    Tick = startOfDay
                })
                .Select(group => new ArticleImpressionDailyModel
                {
                    Tick = group.Key.Tick,
                    OrganizationId = group.Key.OrganizationId,
                    ArticleId = group.Key.ArticleId,
                    ChannelId = group.Key.ChannelId,
                    TotalImpressions = group.Sum(g => g.TotalImpressions), // Sum hourly totals
                    UniqueUsers = group
                        .SelectMany(g => Enumerable.Repeat(g.UniqueUsers, g.TotalImpressions))
                        .Distinct()
                        .Count(), // Approximate unique users
                    AverageImpressionDuration = group
                        .Average(g => g.AverageImpressionDuration * g.TotalImpressions / group.Sum(g => g.TotalImpressions)), // Weighted average
                    TopReferrer = group
                        .GroupBy(g => g.TopReferrer)
                        .OrderByDescending(g => g.Sum(gr => gr.TotalImpressions))
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    TopCountry = group
                        .GroupBy(g => g.TopCountry)
                        .OrderByDescending(g => g.Sum(gr => gr.TotalImpressions))
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    TopCity = group
                        .GroupBy(g => g.TopCity)
                        .OrderByDescending(g => g.Sum(gr => gr.TotalImpressions))
                        .Select(g => g.Key)
                        .FirstOrDefault()
                })
                .ToList();

            // Insert aggregated data into the daily impressions table
            if (dailyImpressions.Any())
            {
                await _dbContext.ArticleImpressionDailyPerformance.AddRangeAsync(dailyImpressions);
                await _dbContext.SaveChangesAsync();
            }

            Console.WriteLine($"Aggregated {dailyImpressions.Count} daily impressions for the day starting {startOfDay}.");

            // Cleanup: Delete hourly impressions older than 14 days in batches
            var hourlyRetentionThreshold = now.AddDays(-14);
            const int batchSize = 1000;

            while (true)
            {
                var oldHourlyImpressions = await _dbContext.ArticleImpressionHourlyPerformance
                    .Where(impression => impression.Tick < hourlyRetentionThreshold)
                    .Take(batchSize)
                    .ToListAsync();

                if (!oldHourlyImpressions.Any())
                    break;

                _dbContext.ArticleImpressionHourlyPerformance.RemoveRange(oldHourlyImpressions);
                var deletedCount = await _dbContext.SaveChangesAsync();

                Console.WriteLine($"Deleted {deletedCount} hourly impressions older than {hourlyRetentionThreshold}.");
            }
        }

        /// <summary>
        /// Aggregate Monthly Impression Data from Daily Data and Clean Up Old Daily Data.
        /// </summary>
        /// <returns></returns>
        public async Task AggregateMonthlyImpressionsAsync()
        {
            // Define the time window for aggregation (previous month)
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1);
            var endOfMonth = startOfMonth.AddMonths(1);

            // Query daily impressions within the time window
            var dailyImpressions = await _dbContext.ArticleImpressionDailyPerformance
                .Where(impression => impression.Tick >= startOfMonth && impression.Tick < endOfMonth)
                .ToListAsync();

            // Group daily data into monthly aggregates
            var monthlyImpressions = dailyImpressions
                .GroupBy(impression => new
                {
                    impression.OrganizationId,
                    impression.ArticleId,
                    impression.ChannelId,
                    Tick = startOfMonth
                })
                .Select(group => new ArticleImpressionMonthlyModel
                {
                    Tick = group.Key.Tick,
                    OrganizationId = group.Key.OrganizationId,
                    ArticleId = group.Key.ArticleId,
                    ChannelId = group.Key.ChannelId,
                    TotalImpressions = group.Sum(g => g.TotalImpressions), // Sum daily totals
                    UniqueUsers = group
                        .SelectMany(g => Enumerable.Repeat(g.UniqueUsers, g.TotalImpressions))
                        .Distinct()
                        .Count(), // Approximate unique users
                    AverageImpressionDuration = group
                        .Average(g => g.AverageImpressionDuration * g.TotalImpressions / group.Sum(g => g.TotalImpressions)), // Weighted average
                    TopReferrer = group
                        .GroupBy(g => g.TopReferrer)
                        .OrderByDescending(g => g.Sum(gr => gr.TotalImpressions))
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    TopCountry = group
                        .GroupBy(g => g.TopCountry)
                        .OrderByDescending(g => g.Sum(gr => gr.TotalImpressions))
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    TopCity = group
                        .GroupBy(g => g.TopCity)
                        .OrderByDescending(g => g.Sum(gr => gr.TotalImpressions))
                        .Select(g => g.Key)
                        .FirstOrDefault()
                })
                .ToList();

            // Insert aggregated data into the monthly impressions table
            if (monthlyImpressions.Any())
            {
                await _dbContext.ArticleImpressionMonthlyPerformance.AddRangeAsync(monthlyImpressions);
                await _dbContext.SaveChangesAsync();
            }

            Console.WriteLine($"Aggregated {monthlyImpressions.Count} monthly impressions for the month starting {startOfMonth}.");

            // Cleanup: Delete daily impressions older than 1 year in batches
            var dailyRetentionThreshold = now.AddYears(-1);
            const int batchSize = 1000;

            while (true)
            {
                var oldDailyImpressions = await _dbContext.ArticleImpressionDailyPerformance
                    .Where(impression => impression.Tick < dailyRetentionThreshold)
                    .Take(batchSize)
                    .ToListAsync();

                if (!oldDailyImpressions.Any())
                    break;

                _dbContext.ArticleImpressionDailyPerformance.RemoveRange(oldDailyImpressions);
                var deletedCount = await _dbContext.SaveChangesAsync();

                Console.WriteLine($"Deleted {deletedCount} daily impressions older than {dailyRetentionThreshold}.");
            }
        }
    }
}
