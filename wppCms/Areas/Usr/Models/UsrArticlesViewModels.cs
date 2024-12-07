using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Usr.Models
{

    public class UsrArticlesCreateEditViewModel : PageViewModel
    {
        public string ChannelId { get; set; }
        public string RowKey { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string? MainImageUrl { get; set; }
        public string? Description { get; set; }
        public string? GithubUrl { get; set; }
        public string AuthorId { get; set; } // The selected author
        public string PermaName { get; set; }
        public string? Tags { get; set; }
        public string? CanonicalUrl { get; set; }
        // Format the dates to be compatible with "datetime-local" input format
        public string PublishSince { get; set; }
        public string? PublishUntil { get; set; }
        public string Folders { get; set; }
        public bool IsHtml { get; set; }
        public bool IsMarkdown { get; set; }
        public string Culture { get; set; }
        public bool IsEditMode { get; set; }
        public string ArticleCulture { get; set; }

        // Add a list of authors to be selected from
        public List<AuthorModel>? Authors { get; set; }
    }
}
