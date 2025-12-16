using System.Collections.Generic;
using System.Threading.Tasks;
using clsCms.Models;

namespace clsCms.Services
{
    public interface IArticleServices
    {
        // Folder CRUD
        Task CreateFolderAsync(FolderModel folder);
        Task<FolderModel> GetFolderAsync(string folderId);
        Task UpdateFolderAsync(FolderModel folder);
        Task DeleteFolderAsync(string folderId);
        Task<List<FolderModel>> ListFoldersAsync(string channelId);

        // Article CRUD
        Task<List<ArticleModel>> GetArticlesByChannelIdAsync(string userId, string channelId);

        
        /// <summary>
        /// Search article from Table.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="searchQuery"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="sort"></param>
        /// <param name="authorPermaName"></param>
        /// <param name="folder"></param>
        /// <param name="isPublishDateSensitive"></param>
        /// <returns></returns>
        Task<PaginationModel<ArticleModel>> SearchArticlesAsync(
     string channelId,
     string searchQuery,
     int currentPage = 1,
     int itemsPerPage = 10,
     string sort = "publishdate_desc",
     string? authorPermaName = null,
     string? folder = null,
     bool isPublishDateSensitive = true);

        /// <summary>
        /// Get distinct list of folders for the channel.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="isPublishDateSensitive"></param>
        /// <returns></returns>
        Task<List<string>> GetFolderFacetsAsync(string channelId, bool isPublishDateSensitive = true);

        /// <summary>
        /// Record search query.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        Task RecordSearchKeywordAsync(string channelId, string keyword);

        /// <summary>
        /// Get list of search history.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        Task<List<SearchQueryHistoryModel>> GetSearchKeywordHistoryAsync(string channelId);

        /// <summary>
        /// Record article impression.
        /// </summary>
        /// <param name="impression"></param>
        /// <returns></returns>
        Task<ArticleImpressionModel> LogArticleImpressionAsync(ArticleImpressionModel impression);

        Task<ArticleModel?> GetArticleByChannelIdAndIdAsync(string channelId, string articleId);

        Task CreateArticleAsync(ArticleModel article);
        Task<bool> IsArticlePermaNameUniqueAsync(string userId, string permaName);
        Task<ArticleModel> GetArticleAsync(string articleId);

        /// <summary>
        /// Retrieves a detailed view of an article, including its content, author information, and associated channel details.
        /// </summary>
        /// <param name="channelId">The ID of the channel where the article resides.</param>
        /// <param name="articleId">The unique identifier of the article to retrieve.</param>
        /// <param name="culture">The culture or language of the article to filter by.</param>
        /// <returns>
        /// An <see cref="ArticleViewModel"/> containing the article's details, author information, and associated channel metadata.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the article with the specified <paramref name="articleId"/> and <paramref name="channelId"/> is not found.
        /// </exception>
        Task<ArticleViewModel> GetArticleViewAsync(string channelId, string articleId, string culture);

        /// <summary>
        /// Retrieves a detailed view of an article by its permalink name, including its content, author information, 
        /// and associated channel details.
        /// </summary>
        /// <param name="channelId">The ID of the channel where the article resides.</param>
        /// <param name="permaName">The permalink name of the article to retrieve.</param>
        /// <param name="culture">The culture or language of the article to filter by.</param>
        /// <param name="isPubslishDateSensitive">
        /// Indicates whether the search should respect the article's publish date. 
        /// Defaults to <c>true</c>, ensuring only currently published articles are retrieved.
        /// </param>
        /// <returns>
        /// An <see cref="ArticleViewModel"/> containing the article's details, author information, and associated channel metadata, 
        /// or <c>null</c> if no matching article is found.
        /// </returns>
        /// <exception cref="Exception">
        /// Thrown when the article with the specified <paramref name="permaName"/> and <paramref name="channelId"/> is not found.
        /// </exception>
        /// <remarks>
        /// This method queries the article table dynamically based on the provided permalink name, 
        /// respecting the <paramref name="isPubslishDateSensitive"/> parameter to include or exclude unpublished articles.
        /// It also fetches additional related data, such as the author and channel information, 
        /// to populate the <see cref="ArticleViewModel"/>.
        /// </remarks>
        Task<ArticleViewModel?> GetArticleViewByPermaNameAsync(string channelId,
            string permaName, string culture, bool isPubslishDateSensitive = true);

        Task UpdateArticleAsync(ArticleModel article);
        Task DeleteArticleAsync(string channelId, string rowKey);
        Task<List<ArticleModel>> ListArticlesAsync(string channelId, List<string> folders);

        // Article Impressions.
        /// <summary>
        /// Record an impression for an article
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="channelId"></param>
        /// <param name="folderId"></param>
        /// <param name="authorId"></param>
        /// <param name="culture"></param>
        /// <param name="tags"></param>
        /// <param name="userAgent"></param>
        /// <param name="browser"></param>
        /// <param name="ipAddress"></param>
        /// <param name="referrer"></param>
        /// <param name="userId"></param>
        /// <param name="deviceId"></param>
        /// <param name="os"></param>
        /// <param name="country"></param>
        /// <param name="city"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        Task<ArticleImpressionModel> UpsertClickImpressionAsync(string articleId,
            string channelId, string folderId, string authorId, string culture,
            string tags, string userAgent, string browser, string ipAddress, string referrer,
            string userId, string deviceId, string os, string country, string city, string language);

        /// <summary>
        /// Get contents of stack barchart for the top page.
        /// </summary>
        /// <param name="organizationId"
        /// <param name="since"></param>
        /// <param name="until"></param>
        /// <returns></returns>
        List<ArticleImpressionByDayViewModel> GetDailyImpressions(string organizationId, DateTimeOffset since, DateTimeOffset until);
    }
}
