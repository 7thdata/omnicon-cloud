using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using clsCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface ISearchServices
    {
        #region manage index
        // --------------------- Index Management ---------------------
        /// <summary>
        /// Creates or updates a search index.
        /// </summary>
        /// <param name="indexName">The name of the index to create or update.</param>
        Task CreateIndexAsync(string indexName);

        /// <summary>
        /// Deletes a search index.
        /// </summary>
        /// <param name="indexName">The name of the index to delete.</param>
        Task DeleteIndexAsync(string indexName);

        /// <summary>
        /// Lists all existing search indexes.
        /// </summary>
        /// <returns>A list of index names.</returns>
        Task<List<string>> ListIndexesAsync();

        /// <summary>
        /// Gets statistics for a specific index.
        /// </summary>
        /// <param name="indexName">The name of the index to retrieve statistics for.</param>
        /// <returns>Index statistics such as document count and storage size.</returns>
        Task<SearchIndexStatistics> GetIndexStatisticsAsync(string indexName);

        // --------------------- Indexer Management ---------------------
        /// <summary>
        /// Creates or updates a search indexer for a specified data source and index.
        /// </summary>
        /// <param name="tableName">The Azure Table Storage table name.</param>
        /// <param name="storageConnectionString">The connection string to the Azure Table Storage.</param>
        Task CreateIndexerAsync(string dataSourceName, string indexName, bool useSchedule = false, string? scheduleInterval = null);

        /// <summary>
        /// Lists all existing indexers.
        /// </summary>
        /// <returns>A list of indexer names.</returns>
        Task<List<string>> ListIndexersAsync();

        /// <summary>
        /// Starts a specific indexer to begin syncing data.
        /// </summary>
        /// <param name="indexerName">The name of the indexer to start.</param>
        Task StartIndexerAsync(string indexerName);

        /// <summary>
        /// Resets a specific indexer, clearing its progress and restarting indexing.
        /// </summary>
        /// <param name="indexerName">The name of the indexer to reset.</param>
        Task ResetIndexerAsync(string indexerName);

        /// <summary>
        /// Deletes a specific indexer.
        /// </summary>
        /// <param name="indexerName">The name of the indexer to delete.</param>
        Task DeleteIndexerAsync(string indexerName);

        // --------------------- Data Source Management ---------------------
        /// <summary>
        /// Creates or updates a data source connection.
        /// </summary>
        /// <param name="dataSourceName">The name of the data source connection.</param>
        /// <param name="tableName">The Azure Table Storage table name.</param>
        /// <param name="storageConnectionString">The connection string to the Azure Table Storage.</param>
        Task CreateDataSourceAsync(string dataSourceName, string tableName, string storageConnectionString);

        /// <summary>
        /// Lists all existing data sources.
        /// </summary>
        /// <returns>A list of data source connection names.</returns>
        Task<List<string>> ListDataSourcesAsync();

        /// <summary>
        /// Deletes a specific data source connection.
        /// </summary>
        /// <param name="dataSourceName">The name of the data source connection to delete.</param>
        Task DeleteDataSourceAsync(string dataSourceName);

        #endregion

        /// <summary>
        /// Searches for articles based on various filtering, sorting, and pagination criteria.
        /// </summary>
        /// <param name="partitionKey">The partition key used to scope the search query.</param>
        /// <param name="query">The full-text search query to filter articles by.</param>
        /// <param name="isArticle">Optional. Indicates whether to filter by articles.</param>
        /// <param name="isArchived">Optional. Indicates whether to filter by archived articles.</param>
        /// <param name="showAuthor">Optional. Indicates whether to filter articles that display author information.</param>
        /// <param name="sort">Optional. The sorting criteria (e.g., "title_asc", "publishdate_desc").</param>
        /// <param name="folder">Optional. The folder name to filter articles by.</param>
        /// <param name="tag">Optional. The tag to filter articles by.</param>
        /// <param name="author">Optional. The author's name to filter articles by.</param>
        /// <param name="pageSize">The number of articles to retrieve per page. Defaults to 10.</param>
        /// <param name="pageNumber">The current page number to retrieve. Defaults to 1.</param>
        /// <param name="culture">Optional. The culture or language code to filter articles by.</param>
        /// <param name="isPublishDateSensitive">
        /// Optional. Indicates whether to filter articles by their publish date. 
        /// Defaults to true, ensuring articles are within the publish date range.
        /// </param>
        /// <returns>
        /// A <see cref="SearchResults{T}"/> containing a list of articles matching the search criteria, 
        /// along with pagination and facet metadata.
        /// </returns>
        /// <remarks>
        /// This method constructs a search query dynamically based on the provided filters and executes 
        /// the query against the search client. It also supports sorting, pagination, and facet extraction 
        /// for tags and folders.
        /// </remarks>
        Task<SearchResults<ArticleSearchModel>?> SearchArticlesAsync(
            string partitionKey,
            string query,
            bool? isArticle = null,
            bool? isArchived = null,
            bool? showAuthor = null,
            string? sort = null,
            string? folder = null,
            string? tag = null,
            string? author = null,
            int pageSize = 10,
            int pageNumber = 1,
            string? culture = null,
            bool isPublishDateSensitive = true);

        /// <summary>
        /// Get just get facets of channel.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="isArticle"></param>
        /// <param name="isArchived"></param>
        /// <param name="showAuthor"></param>
        /// <returns></returns>
        Task<IDictionary<string, IList<FacetResult>>> GetFacetsAsync(
            string partitionKey,
            bool? isArticle = null,
            bool? isArchived = null,
            bool? showAuthor = null);

        /// <summary>
        /// Get related articles.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="currentArticleId"></param>
        /// <param name="tags"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<SearchResults<ArticleSearchModel>?> SearchRelatedArticlesAsync(
    string partitionKey,
    string currentArticleId,
    IEnumerable<string> tags,
    string culture,
    int pageSize = 5);

        /// <summary>
        /// Uploads a collection of articles to the search index.
        /// </summary>
        /// <param name="articles">The collection of articles to index.</param>
        Task IndexArticlesAsync(IEnumerable<ArticleSearchModel> articles);

       
        /// <summary>
        /// Delte collection of documents from the index.
        /// </summary>
        /// <param name="articleIds"></param>
        /// <returns></returns>
        Task UnIndexArticlesAsync(IEnumerable<string> articleIds);
    }
}
