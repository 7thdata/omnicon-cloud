using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace clsCms.Models
{
    /// <summary>
    /// FolderModel - Replaces Category/SubCategory to allow flexible folder structure
    /// </summary>
    public class FolderModel : ITableEntity
    {

        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string PartitionKey { get; set; }
        public string ChannelId { get; set; } // Links to the channel
        public string ParentFolderId { get; set; } // Links to the parent folder, null if it's a root folder
        public string PermaName { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsArchived { get; set; }
    }

    /// <summary>
    /// ArticleModel
    /// </summary>
    public class ArticleModel : ITableEntity
    {
        public string RowKey { get; set; } // Id, must.
        public DateTimeOffset? Timestamp { get; set; } // Automatically managed by Azure Table Storage
        public ETag ETag { get; set; } // Automatically managed by Azure Table Storage
        public string PartitionKey { get; set; } // Channel Id, must.
        public string? MainImageUrl { get; set; }

        public string ChannelId { get; set; }  // The channel the article belongs to, must
        public string Folders { get; set; } // Virtual directory path, separeted by "/", must
        public string PermaName { get; set; } // Permanent URL name, must and must be unique within Partion
        public string Title { get; set; } // Title of the article, must
        public string Text { get; set; } // Article content, optional if github url is provided
        public string Description { get; set; } // SEO Description, must
        public string GithubUrl { get; set; } // If we want text from github, optional
        public string AuthorId { get; set; } // The ID of the author (user), must
        public string AuthorName { get; set; } // The name of the author (user), must, but usually drawn from author id.

        public string? CanonicalUrl { get; set; } // Canonical URL for SEO
        public string Culture { get; set; } // Culture or language code (e.g., "en-US")
        public string? Tags { get; set; } // Tags related to the article, "," separated

        public DateTimeOffset PublishSince { get; set; } // Date from when the article is published, 
        public DateTimeOffset? PublishUntil { get; set; } // Optional: Date until the article is published

        public bool IsHtml { get; set; } // Indicates if the article content is in HTML format,if text is html
        public bool IsMarkdown { get; set; } // Indicates if the article content is in Markdown or other markup,if text is markdown
        public string? RedirectUrl { get; set; } // Redirect URL if the article has been moved
        public bool IsMoved { get; set; } // Indicates if the article has been moved
        public bool IsArchived { get; set; } // Soft delete/archive flag

    }

    /// <summary>
    /// Authors
    /// </summary>
    public class AuthorModel : ITableEntity
    {
        /// <summary>
        /// Unique identifier for the author entity. Used as the Row Key in Azure Table Storage.
        /// </summary>
        public string RowKey { get; set; }

        /// <summary>
        /// Timestamp indicating when the entity was last modified. Managed by Azure Table Storage.
        /// </summary>
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// The ETag used for optimistic concurrency control in Azure Table Storage.
        /// </summary>
        public ETag ETag { get; set; }

        /// <summary>
        /// Partition key for the entity in Azure Table Storage, typically the Channel ID.
        /// </summary>
        public string PartitionKey { get; set; } // Channel Id

        /// <summary>
        /// The ID of the channel that the author is associated with.
        /// </summary>
        public string ChannelId { get; set; }  // The channel the article belongs to

        /// <summary>
        /// The permanent URL-friendly name of the author (used for constructing the author's URL).
        /// </summary>
        public string PermaName { get; set; } // Permanent URL name

        /// <summary>
        /// URL to the author's profile image.
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// The title or name of the author.
        /// </summary>
        public string Title { get; set; } // Author Name

        /// <summary>
        /// A brief introduction or description about the author.
        /// </summary>
        public string Text { get; set; } // Introduction

        /// <summary>
        /// A JSON string that stores the author's social media (SNS) links, which includes the title (name) and URL.
        /// </summary>
        public string SnsLinkJsonString { get; set; } // Author SNS links, consist of title(name), url

        /// <summary>
        /// The canonical URL for SEO purposes. This is the preferred URL that should be indexed by search engines.
        /// </summary>
        public string? CanonicalUrl { get; set; } // Canonical URL for SEO

        /// <summary>
        /// The culture or language code associated with the author's profile (e.g., "en-US").
        /// </summary>
        public string Culture { get; set; } // Culture or language code (e.g., "en-US")

        /// <summary>
        /// Optional tags associated with the author. Can be used for categorization or SEO.
        /// </summary>
        public string? Tags { get; set; } // Tags

        /// <summary>
        /// The date and time from which the author's profile is publicly available.
        /// </summary>
        public DateTimeOffset PublishSince { get; set; } // Date from when profile is available

        /// <summary>
        /// The optional date and time until the author's profile is publicly available. If null, the profile is available indefinitely.
        /// </summary>
        public DateTimeOffset? PublishUntil { get; set; } // Optional: Date until the profile is available

        /// <summary>
        /// URL to redirect the author's profile if the URL has changed or moved.
        /// </summary>
        public string? RedirectUrl { get; set; } // Redirect URL if the profile url has moved

        /// <summary>
        /// Flag indicating whether a 301 redirect is required for the author's profile.
        /// </summary>
        public bool IsMoved { get; set; } // If 301 redirect is needed

        /// <summary>
        /// Flag indicating whether the author's profile is archived (soft-deleted).
        /// </summary>
        public bool IsArchived { get; set; } // Soft delete/archive flag
    }

    public class ArticleViewModel
    {
        public ArticleModel Article { get; set; }
        public AuthorModel Author { get; set; }
        public ChannelViewModel Channel { get; set; }
    }

    /// <summary>
    /// Store article impressions
    /// </summary>
    [Table("ArticleImpressions")]
    public class ArticleImpressionModel
    {
        [Key, MaxLength(36)]
        public string ImpressionId { get; set; }
        [MaxLength(36)]
        public string OrganizationId { get; set; }
        [MaxLength(36)]
        public string ArticleId { get; set; }
        [MaxLength(36)]
        public string ChannelId { get; set; }
        [MaxLength(36)]
        public string FolderId { get; set; }
        [MaxLength(36)]
        public string AuthorId { get; set; }

        [MaxLength(8)]
        public string Culture { get; set; }
        public string? Tags { get; set; }
        public DateTimeOffset ImpressionTime { get; set; }
        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? IpAddress { get; set; }
        public string? Referrer { get; set; }
        public string? UserId { get; set; }
        public string? DeviceId { get; set; }
        public string? Os { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Language { get; set; }
    }

    /// <summary>
    /// For stack bar chart
    /// </summary>
    public class ArticleImpressionByDayViewModel
    {
        public DateTime DateTime { get; set; }
        public List<ArticleImpressionStatsViewModel> Impressions { get; set; }

    }

    /// <summary>
    /// Item of stack bar chart
    /// </summary>
    public class ArticleImpressionStatsViewModel
    {
        public int Impressions { get; set; }
        public string ChannelId { get; set; }
    }
}
