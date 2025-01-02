using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using clsCms.Models;

namespace clsCMs.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserModel>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Hourly Model
            modelBuilder.Entity<ArticleImpressionHourlyModel>(entity =>
            {
                entity.HasKey(e => new { e.Tick, e.OrganizationId, e.ChannelId, e.ArticleId });
            });

            // Daily Model
            modelBuilder.Entity<ArticleImpressionDailyModel>(entity =>
            {
                entity.HasKey(e => new { e.Tick, e.OrganizationId, e.ChannelId, e.ArticleId });
            });

            // Monthly Model
            modelBuilder.Entity<ArticleImpressionMonthlyModel>(entity =>
            {
                entity.HasKey(e => new { e.Tick, e.OrganizationId, e.ChannelId, e.ArticleId });
            });
        }

        // Organizations
        public DbSet<OrganizationModel> Organizations { get; set; }
        public DbSet<OrganizationMemberModel> Memberships { get; set; }

        // Channels
        public DbSet<ChannelModel> Channels { get; set; }
        public DbSet<ChannelMembershipModel> ChannelMemberships { get; set; }

        // Publisher Performance
        public DbSet<ArticleImpressionModel> ArticleImpressions { get; set; }
        public DbSet<ArticleImpressionHourlyModel> ArticleImpressionHourlyPerformance { get; set; }
        public DbSet<ArticleImpressionDailyModel> ArticleImpressionDailyPerformance { get; set; }
        public DbSet<ArticleImpressionMonthlyModel> ArticleImpressionMonthlyPerformance { get; set; }

        // Publisher - AdClicks
        public DbSet<PublisherAdClickModel> PublisherAdClickPerformance { get; set; }
        public DbSet<PublisherAdClickDailyModel> PublisherAdClickDailyPerformance { get; set; }
        public DbSet<PublisherAdClickHourlyModel> PublisherAdClickHourlyPerformance { get; set; }  
        public DbSet<PublisherAdClickMonthlyModel> PublisherAdClickMonthlyModels { get; set; }

        // Publisher - AdImpression
        public DbSet<PublisherAdImpressionModel> PublisherAdImpressionPerformance { get; set; }
        public DbSet<PublisherAdImpressionDailyModel> PublisherAdImpressionDailyPerformance { get; set; }
        public DbSet<PublisherAdImpressionHourlyModel> PublisherAdImpressionHourlyPerformance { get; set; }
        public DbSet<PublisherAdImpressionMonthlyModel> PublisherAdImpressionMonthlyModels { get; set; }

        // Advertiser Performance - Ad Raw Data
        public DbSet<AdClickModel> AdClickPermance { get; set; }
        public DbSet<AdImpressionModel> AdImpressionPerformance { get; set; }

        // Advertiser Performance - Ad Campgigns
        public DbSet<AdCampaignPerformanceRecordMonthlyModel> AdCampaignMonthlyPerformance { get; set; }
        public DbSet<AdCampaignPerformanceRecordDailyModel> AdCampaignDailyPerformance { get; set; }
        public DbSet<AdCampaignPerformanceRecordHourlyModel> AdCampaignHourlyPerformance { get; set; }

        // Advertiser Performance - Ad Creatives
        public DbSet<AdCreativePerformanceRecordMonthlyModel> AdCreativeMonthlyPerformance { get; set; }
        public DbSet<AdCreativePerformanceRecordDailyModel> AdCreativeDailyPerformance { get; set; }
        public DbSet<AdCreativePerformanceRecordHourlyModel> AdCreativeHourlyPerformance { get; set; }

    }
}