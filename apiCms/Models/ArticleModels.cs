using Azure.Search.Documents.Models;
using clsCms.Models;
using System.Text.Json.Serialization;

namespace apiCms.Models
{
    /// <summary>
    /// Represents the request to fetch an article, allowing identification by channel ID and 
    /// either article ID or permalink.
    /// </summary>
    public class GetArticleRequestModel
    {
        /// <summary>
        /// The ID of the channel where the article resides.
        /// </summary>
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// The unique identifier of the article. 
        /// If both ArticleId and ArticlePermaName are provided, ArticleId takes precedence.
        /// </summary>
        [JsonPropertyName("articleId")]
        public string? ArticleId { get; set; }

        /// <summary>
        /// The permalink name of the article, typically used as a human-readable identifier.
        /// </summary>
        [JsonPropertyName("articlePermaName")]
        public string? ArticlePermaName { get; set; }

        [JsonPropertyName("culture")]
        public string Culture { get; set; }
    }

    /// <summary>
    /// Represents the response containing details of a single article and its associated channel.
    /// </summary>
    public class GetArticleResponseModel
    {
        /// <summary>
        /// The details of the article being fetched.
        /// </summary>
        public ArticleViewModel Article { get; set; }

        /// <summary>
        /// The details of the channel where the article resides.
        /// </summary>
        public ChannelViewModel Channel { get; set; }
    }

    /// <summary>
    /// Represents the request for fetching multiple articles with support for filtering, sorting, and pagination.
    /// </summary>
    public class GetArticlesRequestModel
    {
        /// <summary>
        /// The keyword to search for in articles.
        /// </summary>
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }

        /// <summary>
        /// The sorting criteria for the articles (e.g., "date", "popularity").
        /// </summary>
        [JsonPropertyName("sort")]
        public string Sort { get; set; }

        /// <summary>
        /// The ID of the channel to filter articles by.
        /// </summary>
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        /// <summary>
        /// The name of the folder to filter articles by.
        /// </summary>
        [JsonPropertyName("folderName")]
        public string FolderName { get; set; }

        /// <summary>
        /// The permalink name of the author to filter articles by.
        /// </summary>
        [JsonPropertyName("authorPermaName")]
        public string AuthorPermaName { get; set; }

        /// <summary>
        /// The tag to filter articles by.
        /// </summary>
        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        /// <summary>
        /// The culture or language code (e.g., "en-US") to filter articles by.
        /// </summary>
        [JsonPropertyName("culture")]
        public string Culture { get; set; }

        /// <summary>
        /// The number of articles to retrieve per page.
        /// </summary>
        [JsonPropertyName("itemsPerPage")]
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// The current page number to retrieve in the paginated results.
        /// </summary>
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }
    }

    /// <summary>
    /// Represents the response for a request to fetch multiple articles, 
    /// including search results, metadata, and associated details.
    /// </summary>
    public class GetArticlesResponseModel
    {
        /// <summary>
        /// The details of the channel associated with the articles.
        /// </summary>
        public ChannelViewModel Channel { get; set; }

        /// <summary>
        /// The list of articles that match the search criteria.
        /// </summary>
        public List<ArticleSearchModel>? Articles { get; set; }

        /// <summary>
        /// The facets available for refining the search results, represented as a dictionary.
        /// </summary>
        public Dictionary<string, IEnumerable<FacetValue>>? Facets { get; set; }

        /// <summary>
        /// The total number of articles matching the search criteria.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The current page number of the paginated results.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// The number of articles included per page in the results.
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// The keyword used in the search query.
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// The sorting criteria applied to the search results.
        /// </summary>
        public string Sort { get; set; }

        /// <summary>
        /// The profile details of the currently authenticated user, if available.
        /// </summary>
        public ProfileViewModel? CurrentUser { get; set; }

        /// <summary>
        /// The folder filter applied in the search query.
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// The author filter applied in the search query.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The tag filter applied in the search query.
        /// </summary>
        public string Tag { get; set; }
    }

    /// <summary>
    /// Represents the request for fetching multiple articles with support for filtering, sorting, and pagination.
    /// </summary>
    public class GetArticleStatsRequestModel
    {
      

        /// <summary>
        /// The ID of the channel to filter articles by.
        /// </summary>
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }


        /// <summary>
        /// The culture or language code (e.g., "en-US") to filter articles by.
        /// </summary>
        [JsonPropertyName("culture")]
        public string Culture { get; set; }

    }
    /// <summary>
    /// Represents the response containing statistical data for articles, 
    /// including facets for search refinement and associated channel information.
    /// </summary>
    public class GetArticlesStatsResponseModel
    {
        /// <summary>
        /// The facets available for refining the search results, represented as a dictionary
        /// where the key is the facet name, and the value is a collection of facet values.
        /// </summary>
        public Dictionary<string, IEnumerable<FacetValue>>? Facets { get; set; }

        /// <summary>
        /// The details of the channel associated with the statistical data.
        /// </summary>
        public ChannelViewModel Channel { get; set; }
    }
}
