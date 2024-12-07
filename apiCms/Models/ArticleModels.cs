using clsCms.Models;

namespace apiCms.Models
{
    /// <summary>
    /// Represents the request to fetch an article.
    /// </summary>
    public class GetArticleRequestModel
    {
        /// <summary>
        /// The ID of the channel where the article resides.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// The unique identifier of the article. 
        /// If both ArticleId and ArticlePermaName are provided, ArticleId takes precedence.
        /// </summary>
        public string? ArticleId { get; set; }

        /// <summary>
        /// The permalink name of the article.
        /// </summary>
        public string? ArticlePermaName { get; set; }
    }

    public class GetArticleResponseModel
    {
        public ArticleViewModel Article { get; set; }
    }

    public class GetArticlesRequestModel
    {
        public string keyword { get; set; }
        public string sort { get; set; }
        public string channelId { get; set; }
        public string folderId { get; set; }
        public string authorId { get; set; }
        public string culture { get; set; }
        public int itemsPerPage { get; set; }
        public int currentPage { get; set; }
    }

    public class GetArticlesResponseModel
    {
        public PaginationModel<ArticleModel> Articles { get; set; }
    }
}
