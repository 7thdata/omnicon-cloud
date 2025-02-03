using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Models
{
    public class AdCampaignModel : ITableEntity
    {
        // Ad campaign is a group of ad groups
        // has a name, start date, end date, budget, status, etc.
        // This is stored in azure table.
        public string PartitionKey { get; set; } // Organization Id
        public string RowKey { get; set; } // Ad Campaign Id
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } // Automatically managed by Azure Table Storage
        public string Name { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public int Budget { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? StartDateUtc { get; set; }
        public DateTimeOffset? EndDateUtc { get; set; }
        public int CreditRemain { get; set; }
        public bool IgnoreCreditRemain { get; set; }

    }

    public class AdGroupModel : ITableEntity
    {
        // Ad group is a grouof ad creatives under ad campaign
        // has a name, bid, status, etc.
        // This is stored in azure table.
        public string PartitionKey { get; set; } // Organization Id
        public string RowKey { get; set; } // Ad Group Id
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } // Automatically managed by Azure Table Storage
        public string AdCampaignId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Bid { get; set; }
    }

    public class AdGroupAdCreativeMappingModel : ITableEntity
    {
        // Ad group ad creative mapping is the mapping of ad creatives to ad groups
        // This is stored in azure table.
        public string PartitionKey { get; set; } // Organization Id
        public string RowKey { get; set; } // Mapping Id
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } // Automatically managed by Azure Table Storage
        public string AdGroupId { get; set; }
        public string AdCreativeId { get; set; }
    }

    public class AdCreativeModel : ITableEntity
    {
        // Partition and Row Keys for Azure Table Storage
        public string PartitionKey { get; set; } // Organization ID
        public string RowKey { get; set; } // Ad ID

        // General Ad Properties
        public string Name { get; set; } // Name of the Ad Creative
        public string Type { get; set; } // Type of Ad: "Text", "Image", "Banner", etc.

        // Content Fields
        public string Content { get; set; } // Main content for the ad (e.g., HTML for banners)
        public string TextTitle { get; set; } // Title for text ads
        public string TextAdDescription { get; set; } // Description for text ads
        public string ImageAdUrl { get; set; } // URL for image ads
        public string ImageTagUrl { get; set; }

        // Redirect URL for all Ad types
        public string RedirectUrl { get; set; } // Where the ad directs users upon click

        // Metadata Fields for Azure Table
        public DateTimeOffset? Timestamp { get; set; } // Automatically set by Azure Table Storage
        public ETag ETag { get; set; } // Optimistic concurrency control

        // Validation Method
        public bool Validate(out string errorMessage)
        {
            // Check for common validations
            if (string.IsNullOrWhiteSpace(Type))
            {
                errorMessage = "Ad type is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(RedirectUrl))
            {
                errorMessage = "Redirect URL is required.";
                return false;
            }

            // Type-specific validations
            switch (Type.ToLowerInvariant())
            {
                case "text":
                    if (string.IsNullOrWhiteSpace(TextTitle))
                    {
                        errorMessage = "Text Title is required for text ads.";
                        return false;
                    }
                    if (string.IsNullOrWhiteSpace(TextAdDescription))
                    {
                        errorMessage = "Text Ad Description is required for text ads.";
                        return false;
                    }
                    break;

                case "image":
                case "banner":
                    if (string.IsNullOrWhiteSpace(ImageAdUrl))
                    {
                        errorMessage = $"Image Ad URL is required for {Type} ads.";
                        return false;
                    }
                    break;

                default:
                    errorMessage = $"Unsupported ad type: {Type}.";
                    return false;
            }

            errorMessage = null;
            return true;
        }
    }

    public class AdRequestModel
    {
        public string Culture { get; set; }
        public string ChannelId { get; set; }

        public string LocationInclude { get; set; }
        public string LocationExclude { get; set; }
        public string CategoryInclude { get; set; }
        public string CategoryExclude { get; set; }
        public string AdType { get; set; }
        public string YourCurrentCountry { get; set; }  
        public string YourCurrentRegion { get; set; }
    }

    public class AdServingModel
    {
        public AdCampaignModel AdCampaign { get; set; }
        public AdGroupModel AdGroup { get; set; }
        public AdCreativeModel AdCreative { get; set; }
        public string Culture { get; set; }
    }


    /// <summary>
    /// Raw impression data.
    /// Keep it up to 7 days.
    /// </summary>
    [Table("AdImpressions")]
    public class AdImpressionModel
    {
        // Ad impression is the record of the ad being shown to the user
        // This is stoerd in SQL database
        [Key, MaxLength(36)]
        public string ImpressionId { get; set; }
        [MaxLength(64)]
        public string OrganizationId { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? Referrer { get; set; }
        public string? AdCampaignId { get; set; }
        public string? AdGroupId { get; set; }
        public string? AdId { get; set; }
        public string? UserId { get; set; }
        public string? DeviceId { get; set; }
        public string? Browser { get; set; }
        public string? Os { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Language { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceBrand { get; set; }
        public int Impressions { get; set; }
        public int CPM { get; set; }
    }

    /// <summary>
    /// Raw click data.
    /// Keep it up to 7 days.
    /// </summary>
    [Table("AdClicks")]
    public class AdClickModel
    {
        // Ad click is the record of the user clicking on the ad
        // This is stored in SQL database
        [Key, MaxLength(36)]
        public string ClickId { get; set; }
        [MaxLength(36)]
        public string OrganizationId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }
        public string? Referrer { get; set; }
        public string? AdCampaignId { get; set; }
        public string? AdGroupId { get; set; }

        public string? AdId { get; set; }
        public string? UserId { get; set; }
        public string? DeviceId { get; set; }
        public string? Browser { get; set; }
        public string? Os { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Language { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceBrand { get; set; }
        public int Clicks { get; set; } // Times clicked
        public int Amount { get; set; } // Cost for the click

    }

    /// <summary>
    /// Store hourly record.
    /// Keep it up to 336 hours
    /// </summary>
    [Table("AdCampaignPerformanceHourly")]
    public class AdCampaignPerformanceRecordHourlyModel
    {
        [Key, MaxLength(64)]
        public string Id { get; set; } // yyyy-MM-dd-campaignId
        [MaxLength(64)]
        public string OrganizationId { get; set; }
        public DateTime Tick { get; set; } //
        public int Impressions { get; set; }
        public int CPM { get; set; }
        public int Clicks { get; set; }
        public int CPC { get; set; }
        [MaxLength(64)]
        public string ChannelId { get; set; }
        [MaxLength(64)]
        public string AdCampaignId { get; set; }
        [MaxLength(64)]
        public string AdGroupId { get; set; }
    }

    /// <summary>
    /// Store hourly record  
    /// Keep it up to 336 hours
    /// </summary>
    [Table("AdCreativePerformanceHourly")]
    public class AdCreativePerformanceRecordHourlyModel
    {
        [Key, MaxLength(64)]
        public string Id { get; set; }
        [MaxLength(64)]
        public string OrganizationId { get; set; }
        public DateTime Tick { get; set; }
        public int Impressions { get; set; }
        public int CPM { get; set; }
        public int Clicks { get; set; }
        public int CPC { get; set; }
        [MaxLength(64)]
        public string AdGreativeId { get; set; }
    }

    /// <summary>
    /// Store hourly record.
    /// Keep it up to 1 year
    /// </summary>
    [Table("AdCampaignPerformanceDaily")]
    public class AdCampaignPerformanceRecordDailyModel
    {
        [Key, MaxLength(64)]
        public string Id { get; set; } // yyyy-MM-dd-campaignId
        [MaxLength(64)]
        public string OrganizationId { get; set; }
        public DateTime Tick { get; set; } //
        public int Impressions { get; set; }
        public int CPM { get; set; }
        public int Clicks { get; set; }
        public int CPC { get; set; }
        [MaxLength(64)]
        public string ChannelId { get; set; }
        [MaxLength(64)]
        public string AdCampaignId { get; set; }
        [MaxLength(64)]
        public string AdGroupId { get; set; }
    }

    /// <summary>
    /// Store hourly
    /// Keep it up to 1 year
    /// </summary>
    [Table("AdCreativePerformanceDaily")]
    public class AdCreativePerformanceRecordDailyModel
    {
        [Key, MaxLength(64)]
        public string Id { get; set; }
        [MaxLength(64)]
        public string OrganizationId { get; set; }
        public DateTime Tick { get; set; }
        public int Impressions { get; set; }
        public int CPM { get; set; }
        public int Clicks { get; set; }
        public int CPC { get; set; }
        [MaxLength(64)]
        public string AdGreativeId { get; set; }
    }

    /// <summary>
    /// Store monthly campaign performance
    /// </summary>
    [Table("AdCampaignPerformanceMonthly")]
    public class AdCampaignPerformanceRecordMonthlyModel
    {
        [Key, MaxLength(64)]
        public string Id { get; set; } // yyyy-MM-dd-campaignId
        [MaxLength(64)]
        public string OrganizationId { get; set; }
        public DateTime Tick { get; set; } //
        public int Impressions { get; set; }
        public int CPM { get; set; }
        public int Clicks { get; set; }
        public int CPC { get; set; }
        [MaxLength(64)]
        public string ChannelId { get; set; }
        [MaxLength(64)]
        public string AdCampaignId { get; set; }
        [MaxLength(64)]
        public string AdGroupId { get; set; }
    }

    /// <summary>
    /// Store monthly ad performance.
    /// </summary>
    [Table("AdCreativePerformanceMonthly")]
    public class AdCreativePerformanceRecordMonthlyModel
    {
        [Key, MaxLength(64)]
        public string Id { get; set; }
        [MaxLength(64)]
        public string OrganizationId { get; set; }
        public DateTime Tick { get; set; }
        public int Impressions { get; set; }
        public int CPM { get; set; }
        public int Clicks { get; set; }
        public int CPC { get; set; }
        [MaxLength(64)]
        public string AdGreativeId { get; set; }
    }
}
