using Azure;
using Azure.Data.Tables;
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
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? StartDateUtc { get; set; }   
        public DateTimeOffset? EndDateUtc { get; set; }

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
        // Ad create is the actual ad that is shown to the user
        // This is stored in azure table.
        public string PartitionKey { get; set; } // Organization Id
        public string RowKey { get; set; } // Ad Id
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; } // Automatically managed by Azure Table Storage
    }

    [Table("AdImpressions")]
    public class AdImpressionModel
    {
        // Ad impression is the record of the ad being shown to the user
        // This is stoerd in SQL database
        [Key,MaxLength(36)]
        public string ImpressionId { get; set; }
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

    [Table("AdClicks")]
    public class AdClickModel
    {
        // Ad click is the record of the user clicking on the ad
        // This is stored in SQL database
        [Key,MaxLength(36)]
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
}
