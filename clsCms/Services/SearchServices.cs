using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.Extensions.Options;

namespace clsCms.Services
{
    /// <summary>
    /// Provides Azure Cognitive Search integration for article content.
    ///
    /// Responsibilities:
    /// - Index & indexer lifecycle management
    /// - Full-text article search
    /// - Faceted navigation (tags, folders)
    /// - Related article discovery
    /// - Bulk indexing / de-indexing
    ///
    /// This service intentionally encapsulates Azure Search SDK usage
    /// to keep controllers and domain services search-agnostic.
    /// </summary>
    public class SearchServices : ISearchServices
    {
        private readonly SearchIndexClient _indexClient;
        private readonly SearchIndexerClient _indexerClient;
        private readonly SearchClient _searchClient;

        private readonly IChannelServices _channelServices;
        private readonly IArticleServices _articleServices;

        private readonly string _indexName;

        /// <summary>
        /// Initializes Azure Cognitive Search clients using application configuration.
        /// </summary>
        public SearchServices(
            IOptions<AppConfigModel> appConfig,
            IChannelServices channelServices,
            IArticleServices articleServices)
        {
            if (appConfig?.Value?.AzureSearch == null)
                throw new ArgumentNullException(nameof(appConfig), "Azure Search configuration is missing.");

            var config = appConfig.Value.AzureSearch;

            if (string.IsNullOrWhiteSpace(config.SearchServiceName))
                throw new ArgumentException("SearchServiceName is required.");

            if (string.IsNullOrWhiteSpace(config.SearchApiKey))
                throw new ArgumentException("SearchApiKey is required.");

            _indexName = config.IndexName ?? "articles";

            var endpoint = new Uri($"https://{config.SearchServiceName}.search.windows.net");
            var credential = new AzureKeyCredential(config.SearchApiKey);

            _indexClient = new SearchIndexClient(endpoint, credential);
            _indexerClient = new SearchIndexerClient(endpoint, credential);
            _searchClient = new SearchClient(endpoint, _indexName, credential);

            _channelServices = channelServices;
            _articleServices = articleServices;
        }

        #region Index management

        /// <summary>
        /// Creates or updates the article search index.
        /// Uses <see cref="ArticleSearchModel"/> as the schema source.
        /// </summary>
        public async Task CreateIndexAsync(string indexName)
        {
            var fieldBuilder = new FieldBuilder();
            var fields = fieldBuilder.Build(typeof(ArticleSearchModel));

            var index = new SearchIndex(indexName)
            {
                Fields = fields,

                // Suggester is intentionally limited to Title
                // to avoid incompatible collection fields.
                Suggesters =
                {
                    new SearchSuggester("sg", new[] { "Title" })
                }
            };

            await _indexClient.CreateOrUpdateIndexAsync(index);
        }

        /// <summary>
        /// Deletes a search index.
        /// </summary>
        public async Task DeleteIndexAsync(string indexName)
        {
            await _indexClient.DeleteIndexAsync(indexName);
        }

        /// <summary>
        /// Lists all search indexes in the search service.
        /// </summary>
        public async Task<List<string>> ListIndexesAsync()
        {
            return _indexClient
                .GetIndexes()
                .Select(i => i.Name)
                .ToList();
        }

        /// <summary>
        /// Retrieves index statistics such as document count and storage size.
        /// </summary>
        public async Task<SearchIndexStatistics> GetIndexStatisticsAsync(string indexName)
        {
            var response = await _indexClient.GetIndexStatisticsAsync(indexName);
            return response.Value;
        }

        #endregion

        #region Indexer & data source management

        /// <summary>
        /// Creates or updates an Azure Search indexer.
        /// Indexers are typically used for automated ingestion from storage.
        /// </summary>
        public async Task CreateIndexerAsync(
            string dataSourceName,
            string indexName,
            bool useSchedule = false,
            string? scheduleInterval = null)
        {
            if (string.IsNullOrWhiteSpace(dataSourceName))
                throw new ArgumentException("Data source name is required.", nameof(dataSourceName));

            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name is required.", nameof(indexName));

            var indexerName = $"{dataSourceName}-indexer";
            var indexer = new SearchIndexer(indexerName, dataSourceName, indexName);

            if (useSchedule && !string.IsNullOrWhiteSpace(scheduleInterval))
            {
                indexer.Schedule = new IndexingSchedule(TimeSpan.Parse(scheduleInterval))
                {
                    StartTime = DateTimeOffset.UtcNow
                };
            }

            await _indexerClient.CreateOrUpdateIndexerAsync(indexer);
        }

        /// <summary>
        /// Lists all indexers registered in the search service.
        /// </summary>
        public async Task<List<string>> ListIndexersAsync()
        {
            var indexers = await _indexerClient.GetIndexersAsync();
            return indexers.Value.Select(i => i.Name).ToList();
        }

        /// <summary>
        /// Manually triggers an indexer run.
        /// </summary>
        public async Task StartIndexerAsync(string indexerName)
        {
            await _indexerClient.RunIndexerAsync(indexerName);
        }

        /// <summary>
        /// Resets indexer state (forces full reprocessing on next run).
        /// </summary>
        public async Task ResetIndexerAsync(string indexerName)
        {
            await _indexerClient.ResetIndexerAsync(indexerName);
        }

        /// <summary>
        /// Deletes an indexer.
        /// </summary>
        public async Task DeleteIndexerAsync(string indexerName)
        {
            await _indexerClient.DeleteIndexerAsync(indexerName);
        }

        /// <summary>
        /// Creates or updates a data source connection.
        /// Typically used with Azure Table Storage.
        /// </summary>
        public async Task CreateDataSourceAsync(
            string dataSourceName,
            string tableName,
            string storageConnectionString)
        {
            var container = new SearchIndexerDataContainer(tableName);

            var dataSource = new SearchIndexerDataSourceConnection(
                name: dataSourceName,
                type: SearchIndexerDataSourceType.AzureTable,
                connectionString: storageConnectionString,
                container: container);

            await _indexerClient.CreateOrUpdateDataSourceConnectionAsync(dataSource);
        }

        /// <summary>
        /// Lists all configured data sources.
        /// </summary>
        public async Task<List<string>> ListDataSourcesAsync()
        {
            var sources = await _indexerClient.GetDataSourceConnectionsAsync();
            return sources.Value.Select(s => s.Name).ToList();
        }

        /// <summary>
        /// Deletes a data source.
        /// </summary>
        public async Task DeleteDataSourceAsync(string dataSourceName)
        {
            await _indexerClient.DeleteDataSourceConnectionAsync(dataSourceName);
        }

        #endregion

        #region Reindexing & upload

        /// <summary>
        /// Reindexes all searchable articles across all channels.
        ///
        /// This is an expensive operation and should be used
        /// for maintenance or recovery scenarios only.
        /// </summary>
        public async Task Reindex()
        {
            var channels = await _channelServices.GetAllChannelsAsync(
                keyword: "",
                sort: "",
                organizationId: "",
                currentPage: 1,
                itemsPerPage: 1000);

            foreach (var channel in channels.Items)
            {
                var articles = await _articleServices
                    .GetArticlesByChannelIdAsync("", channel.Id);

                var documents = articles
                    .Where(a =>
                        a.IsArchived &&
                        !a.IsMoved &&
                        a.IsSearchable)
                    .Select(a => new ArticleSearchModel
                    {
                        PartitionKey = a.ChannelId,
                        RowKey = a.RowKey,
                        Title = a.Title,
                        Text = a.Text,
                        Description = a.Description,
                        Tags = a.Tags?.Split(','),
                        Folders = a.Folders,
                        AuthorName = a.AuthorName,
                        ShowAuthor = a.ShowAuthor,
                        IsArticle = a.IsArticle,
                        IsArchived = a.IsArchived,
                        Culture = a.Culture,
                        CanonicalUrl = a.CanonicalUrl,
                        MainImageUrl = a.MainImageUrl,
                        PublishSince = a.PublishSince,
                        PublishUntil = a.PublishUntil,
                        PermaName = a.PermaName,
                        Version = a.Version,
                        UpdatedAt = a.UpdatedAt
                    })
                    .ToList();

                await IndexArticlesAsync(documents);
            }
        }

        #endregion

        #region Search

        /// <summary>
        /// Performs a full-text article search with filters, sorting, pagination and facets.
        /// </summary>
        public async Task<SearchResults<ArticleSearchModel>?> SearchArticlesAsync(
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
            bool isPublishDateSensitive = true)
        {
            var filters = new List<string>
            {
                $"PartitionKey eq '{partitionKey}'"
            };

            if (isArticle.HasValue) filters.Add($"IsArticle eq {isArticle.Value.ToString().ToLower()}");
            if (isArchived.HasValue) filters.Add($"IsArchived eq {isArchived.Value.ToString().ToLower()}");
            if (showAuthor.HasValue) filters.Add($"ShowAuthor eq {showAuthor.Value.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(folder)) filters.Add($"Folders eq '{folder}'");
            if (!string.IsNullOrWhiteSpace(tag)) filters.Add($"Tags/any(t: t eq '{tag}')");
            if (!string.IsNullOrWhiteSpace(author)) filters.Add($"AuthorName eq '{author}'");
            if (!string.IsNullOrWhiteSpace(culture)) filters.Add($"Culture eq '{culture}'");

            if (isPublishDateSensitive)
            {
                var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                filters.Add($"PublishSince le {now}");
                filters.Add($"PublishUntil ge {now}");
            }

            var options = new SearchOptions
            {
                Filter = string.Join(" and ", filters),
                IncludeTotalCount = true,
                Size = pageSize,
                Skip = (pageNumber - 1) * pageSize
            };

            options.Select.Add("Title");
            options.Select.Add("Folders");
            options.Select.Add("Tags");
            options.Select.Add("PublishSince");
            options.Select.Add("PermaName");
            options.Select.Add("MainImageUrl");
            options.Select.Add("Description");
            options.Select.Add("Culture");

            options.Facets.Add("Tags");
            options.Facets.Add("Folders");

            switch (sort?.ToLower())
            {
                case "title_asc": options.OrderBy.Add("Title asc"); break;
                case "title_desc": options.OrderBy.Add("Title desc"); break;
                case "publishdate_asc": options.OrderBy.Add("PublishSince asc"); break;
                case "publishdate_desc": options.OrderBy.Add("PublishSince desc"); break;
            }

            var result = await _searchClient.SearchAsync<ArticleSearchModel>(query, options);
            return result.Value?.TotalCount > 0 ? result.Value : null;
        }

        /// <summary>
        /// Retrieves only facet data (tags, folders) for a channel.
        /// </summary>
        public async Task<IDictionary<string, IList<FacetResult>>> GetFacetsAsync(
            string partitionKey,
            bool? isArticle = null,
            bool? isArchived = null,
            bool? showAuthor = null)
        {
            var filters = new List<string>
            {
                $"PartitionKey eq '{partitionKey}'"
            };

            if (isArticle.HasValue) filters.Add($"IsArticle eq {isArticle.Value.ToString().ToLower()}");
            if (isArchived.HasValue) filters.Add($"IsArchived eq {isArchived.Value.ToString().ToLower()}");
            if (showAuthor.HasValue) filters.Add($"ShowAuthor eq {showAuthor.Value.ToString().ToLower()}");

            var options = new SearchOptions
            {
                Filter = string.Join(" and ", filters),
                Size = 0
            };

            options.Facets.Add("Tags");
            options.Facets.Add("Folders");

            var results = await _searchClient.SearchAsync<ArticleSearchModel>("*", options);
            return results.Value.Facets;
        }

        /// <summary>
        /// Finds related articles based on shared tags.
        /// </summary>
        public async Task<SearchResults<ArticleSearchModel>?> SearchRelatedArticlesAsync(
            string partitionKey,
            string currentArticleId,
            IEnumerable<string> tags,
            string culture,
            int pageSize = 5)
        {
            if (tags == null || !tags.Any())
                return null;

            var now = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var filters = new List<string>
            {
                $"PartitionKey eq '{partitionKey}'",
                $"RowKey ne '{currentArticleId}'",
                $"Culture eq '{culture}'",
                $"PublishSince le {now}",
                $"PublishUntil ge {now}",
                $"({string.Join(" or ", tags.Select(t => $"Tags/any(x: x eq '{t}')"))})"
            };

            var options = new SearchOptions
            {
                Filter = string.Join(" and ", filters),
                Size = pageSize,
                IncludeTotalCount = true
            };

            options.Select.Add("Title");
            options.Select.Add("PublishSince");
            options.Select.Add("Culture");
            options.Select.Add("PermaName");
            options.Select.Add("Tags");
            options.Select.Add("MainImageUrl");

            var result = await _searchClient.SearchAsync<ArticleSearchModel>("*", options);
            return result.Value;
        }

        #endregion

        #region Index updates

        /// <summary>
        /// Uploads or updates article documents in the search index.
        /// </summary>
        public async Task IndexArticlesAsync(IEnumerable<ArticleSearchModel> articles)
        {
            await _searchClient.UploadDocumentsAsync(articles);
        }

        /// <summary>
        /// Removes articles from the search index by RowKey.
        /// </summary>
        public async Task UnIndexArticlesAsync(IEnumerable<string> articleIds)
        {
            var keys = articleIds.Select(id => new { RowKey = id });
            await _searchClient.DeleteDocumentsAsync(keys);
        }

        #endregion
    }
}
