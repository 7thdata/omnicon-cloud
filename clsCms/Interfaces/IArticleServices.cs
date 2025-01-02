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
        /// Search for article.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="searchQuery"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="sort"></param>
        /// <param name="authorPermaName"></param>
        /// <param name="isPublishDateSensitive"></param>
        /// <returns></returns>
        Task<PaginationModel<ArticleModel>> SearchArticlesAsync(
     string channelId,
     string searchQuery,
     int currentPage = 1,
     int itemsPerPage = 10,
     string sort = "publishdate_desc",
     string authorPermaName = null,
     bool isPublishDateSensitive = true);

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
        /// Get article view.
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        Task<ArticleViewModel> GetArticleViewAsync(string channelId, string articleId);

        /// <summary>
        /// Get article view by perma name.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="permaName"></param>
        /// <param name="isPubslishDateSensitive"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        Task<ArticleViewModel?> GetArticleViewByPermaNameAsync(string channelId,
            string permaName, bool isPubslishDateSensitive = true);

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
