using wppCms.Models;
using clsCms.Models;
using System.ComponentModel.DataAnnotations;

namespace wppCms.Areas.Usr.Models
{
    public class UsrHomeIndexViewModel : PageViewModel
    {
        // List of channels belonging to the logged-in user
        public PaginationModel<ChannelViewModel> Channels { get; set; }

        // Any additional properties you want to add for the dashboard
        public string WelcomeMessage { get; set; } = "Welcome to your CMS Dashboard!";
    }

    public class UsrHomeChannelViewModel : PageViewModel
    {
        public ChannelModel Channel { get; set; } // Selected channel details
        public List<ArticleModel> Articles { get; set; } // List of articles under the channel
    }

    public class UsrHomeChannelDetailsViewModel : PageViewModel
    {

        public ChannelViewModel Channel { get; set; }
        
    }

    public class UsrHomeArticleDetailsViewModel : PageViewModel
    {
        public ArticleModel Article { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsEditable { get; set; }
    }

    public class UsrHomeArticleCreateViewModel : PageViewModel
    {
        public string RowKey { get; set; } // Unique identifier for the article
        public string ChannelId { get; set; } // ID of the channel
        public string Title { get; set; } // Title of the article
        public string Text { get; set; } // Content of the article
        public string PermaName { get; set; } // Unique perma name for the article URL
        public string FolderPath { get; set; } // Path for folders/categories
        public string? Tags { get; set; } // Tags for the article
        public string? CanonicalUrl { get; set; } // Optional canonical URL for SEO
        public string Culture { get; set; } // Culture or language of the article

        public DateTimeOffset PublishSince { get; set; } // When the article should be published
        public DateTimeOffset? PublishUntil { get; set; } // Optional: When the article should be unpublished

        public bool IsHtml { get; set; } // Whether the content is in HTML
        public bool IsMarkup { get; set; } // Whether the content is in Markdown or other markup



        // Flags to indicate the current mode
        public bool IsEditMode { get; set; } // Whether the form is in edit mode
        public bool IsReadOnly { get; set; } // Whether the form is read-only
    }

    public class UsrHomeArticleFormViewModel
    {
        // Article Identification
        public string ChannelId { get; set; }
        public string RowKey { get; set; }

        // Article Content
        [Required]
        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public string FolderPath { get; set; }

        [Required]
        [RegularExpression("[a-z0-9\\-]+", ErrorMessage = "PermaName must be lowercase and contain only letters, numbers, and hyphens.")]
        public string PermaName { get; set; }

        public string Tags { get; set; }
        public string CanonicalUrl { get; set; }

        // Publish Date
        [Required]
        public DateTimeOffset PublishSince { get; set; }

        // Archive Status
        public bool IsArchived { get; set; }

        // Access Statistics
        public int ViewsCount { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }

        // Localization
        [Required]
        public string Culture { get; set; }
    }
}
