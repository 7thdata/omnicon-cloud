using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Azure;
using clsCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using clsCms.Interfaces;
using Microsoft.Extensions.Options;


namespace clsCms.Services
{
    /// <summary>
    /// Handles Azure Cognitive Search operations for articles.
    /// </summary>
    public class SearchServices : ISearchServices
    {
        private readonly string _searchServiceEndpoint;
        private readonly string _searchServiceApiKey;
        private readonly string _indexName = "articles";
        private readonly SearchIndexClient _indexClient;
        private readonly SearchIndexerClient _indexerClient;
        private readonly SearchClient _searchClient;
        private readonly IChannelServices _channelServices;
        private readonly IArticleServices _articleServices;
        private readonly IOptions<AppConfigModel> _appConfig;

        public SearchServices(IOptions<AppConfigModel> appConfig, IChannelServices channelServices, IArticleServices articleServices)
        {
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig), "AppConfigModel cannot be null.");

            var azureSearchConfig = _appConfig.Value.AzureSearch ?? throw new ArgumentNullException(nameof(appConfig.Value.AzureSearch), "Azure Search configuration is missing.");

            _searchServiceEndpoint = $"https://{azureSearchConfig.SearchServiceName}.search.windows.net";
            _searchServiceApiKey = azureSearchConfig.SearchApiKey ?? throw new ArgumentNullException(nameof(azureSearchConfig.SearchApiKey), "Azure Search API Key cannot be null.");
            _indexName = azureSearchConfig.IndexName ?? "default-index"; // Default index name if not specified

            // Initialize Azure Cognitive Search clients
            _indexClient = new SearchIndexClient(new Uri(_searchServiceEndpoint), new AzureKeyCredential(_searchServiceApiKey));
            _indexerClient = new SearchIndexerClient(new Uri(_searchServiceEndpoint), new AzureKeyCredential(_searchServiceApiKey));
            _searchClient = new SearchClient(new Uri(_searchServiceEndpoint), _indexName, new AzureKeyCredential(_searchServiceApiKey));
            _channelServices = channelServices;
            _articleServices = articleServices;
        }

        #region Index Management
        public async Task CreateIndexAsync(string indexName)
        {
            var fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(ArticleSearchModel));

            // Only include fields compatible with the suggester
            var index = new SearchIndex(indexName)
            {
                Fields = searchFields,
                Suggesters =
        {
            new SearchSuggester("sg", new[] { "Title" }) // Removed "Tags" to ensure compatibility
        }
            };

            await _indexClient.CreateOrUpdateIndexAsync(index);
            Console.WriteLine($"Index '{indexName}' created or updated.");
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            await _indexClient.DeleteIndexAsync(indexName);
            Console.WriteLine($"Index '{indexName}' deleted.");
        }

        public async Task<List<string>> ListIndexesAsync()
        {
            var indexes = _indexClient.GetIndexes();
            return indexes.Select(index => index.Name).ToList();
        }

        public async Task<SearchIndexStatistics> GetIndexStatisticsAsync(string indexName)
        {
            var response = await _indexClient.GetIndexStatisticsAsync(indexName);
            return response.Value;
        }

        // --------------------- Indexer Management ---------------------
        public async Task CreateIndexerAsync(string dataSourceName, string indexName, bool useSchedule = false, string? scheduleInterval = null)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(dataSourceName))
                    throw new ArgumentException("Data source name cannot be null or empty.", nameof(dataSourceName));

                if (string.IsNullOrEmpty(indexName))
                    throw new ArgumentException("Index name cannot be null or empty.", nameof(indexName));

                // Define the indexer
                var indexerName = $"{dataSourceName}-indexer";
                var indexer = new SearchIndexer(indexerName, dataSourceName, indexName);

                // Optional: Configure a schedule if required
                if (useSchedule && !string.IsNullOrEmpty(scheduleInterval))
                {
                    var schedule = new IndexingSchedule(TimeSpan.Parse(scheduleInterval))
                    {
                        StartTime = DateTimeOffset.Now,
                    };
                    indexer.Schedule = schedule;
                }

                // Create or update the indexer
                await _indexerClient.CreateOrUpdateIndexerAsync(indexer);
                Console.WriteLine($"Indexer '{indexer.Name}' created or updated.");
            }
            catch (RequestFailedException ex)
            {
                Console.Error.WriteLine($"Azure Search Service error: {ex.Message}");
                throw; // Re-throw the exception for higher-level handling
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                throw; // Re-throw unexpected errors
            }
        }
        public async Task<List<string>> ListIndexersAsync()
        {
            var indexers = await _indexerClient.GetIndexersAsync();
            return indexers.Value.Select(indexer => indexer.Name).ToList();
        }

        public async Task StartIndexerAsync(string indexerName)
        {
            await _indexerClient.RunIndexerAsync(indexerName);
            Console.WriteLine($"Indexer '{indexerName}' started.");
        }

        public async Task ResetIndexerAsync(string indexerName)
        {
            await _indexerClient.ResetIndexerAsync(indexerName);
            Console.WriteLine($"Indexer '{indexerName}' reset.");
        }

        public async Task DeleteIndexerAsync(string indexerName)
        {
            await _indexerClient.DeleteIndexerAsync(indexerName);
            Console.WriteLine($"Indexer '{indexerName}' deleted.");
        }

        // --------------------- Data Source Management ---------------------
        public async Task CreateDataSourceAsync(string dataSourceName, string tableName, string storageConnectionString)
        {
            var dataContainer = new SearchIndexerDataContainer(tableName)
            {
                Query = ""
            };

            var dataSource = new SearchIndexerDataSourceConnection(
                name: dataSourceName,
                type: SearchIndexerDataSourceType.AzureTable,
                connectionString: storageConnectionString,
                container: dataContainer);

            await _indexerClient.CreateOrUpdateDataSourceConnectionAsync(dataSource);
            Console.WriteLine($"Data source '{dataSourceName}' created or updated.");
        }

        public async Task<List<string>> ListDataSourcesAsync()
        {
            var dataSources = await _indexerClient.GetDataSourceConnectionsAsync();
            return dataSources.Value.Select(dataSource => dataSource.Name).ToList();
        }

        public async Task DeleteDataSourceAsync(string dataSourceName)
        {
            await _indexerClient.DeleteDataSourceConnectionAsync(dataSourceName);
            Console.WriteLine($"Data source '{dataSourceName}' deleted.");
        }

        #endregion


        #region search and upload

        public async Task Reindex()
        {
            // Go through all channels and reindex them
            var channels = await _channelServices.AdminGetAllChannels();

            foreach(var channel in channels)
            {
                var articles = await _articleServices.GetArticlesByChannelIdAsync("",channel.Id);

                var articlesToIndex = new List<ArticleSearchModel>();

                foreach (var article in articles)
                {
                    if (article.IsArchived && !article.IsMoved && article.IsSearchable)
                    {
                        var searchModel = new ArticleSearchModel()
                        {
                            IsArticle = article.IsArticle,
                            AuthorName = article.AuthorName,
                            IsArchived = article.IsArchived,
                            ShowAuthor = article.ShowAuthor,
                            UpdatedAt = article.UpdatedAt,
                            CanonicalUrl = article.CanonicalUrl,
                            Culture = article.Culture,
                            Description = article.Description,
                            Folders = article.Folders,
                            MainImageUrl = article.MainImageUrl,
                            PartitionKey = article.ChannelId,
                            PermaName = article.PermaName,
                            PublishSince = article.PublishSince,
                            PublishUntil = article.PublishUntil,
                            RowKey = article.RowKey,
                            Tags = article.Tags?.Split(","),
                            Text = article.Text,
                            Title = article.Title,
                            Version = article.Version
                        };

                        articlesToIndex.Add(searchModel);
                    }
                }

                await IndexArticlesAsync(articlesToIndex);
            }
        }

        // --------------------- Search and Upload ---------------------
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
            // Initialize filter clauses with PartitionKey
            var filterClauses = new List<string>
            {
                $"PartitionKey eq '{partitionKey}'"
            };

            // Add optional filters
            if (isArticle.HasValue) filterClauses.Add($"IsArticle eq {isArticle.Value.ToString().ToLower()}");
            if (isArchived.HasValue) filterClauses.Add($"IsArchived eq {isArchived.Value.ToString().ToLower()}");
            if (showAuthor.HasValue) filterClauses.Add($"ShowAuthor eq {showAuthor.Value.ToString().ToLower()}");
            if (!string.IsNullOrWhiteSpace(folder)) filterClauses.Add($"Folders eq '{folder}'");
            if (!string.IsNullOrWhiteSpace(tag)) filterClauses.Add($"Tags/any(t: t eq '{tag}')");
            if (!string.IsNullOrWhiteSpace(author)) filterClauses.Add($"AuthorName eq '{author}'");
            if (!string.IsNullOrWhiteSpace(culture))filterClauses.Add($"Culture eq '{culture}'");

            if (isPublishDateSensitive)
            {
                filterClauses.Add($"PublishSince le {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
                filterClauses.Add($"PublishUntil ge {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            }

            // Combine all filter clauses
            var filter = string.Join(" and ", filterClauses);

            // Configure search options
            var options = new SearchOptions
            {
                Filter = filter,
                IncludeTotalCount = true
            };

            if (pageSize > 0)
            {
                options.Size = pageSize;
                options.Skip = (pageNumber - 1) * pageSize;
            }

            // Add fields to select
            options.Select.Add("Title");
            options.Select.Add("Folders");
            options.Select.Add("Tags");
            options.Select.Add("PublishSince");
            options.Select.Add("PermaName");
            options.Select.Add("MainImageUrl");
            options.Select.Add("Description");
            options.Select.Add("Culture");

            // Add facets
            options.Facets.Add("Tags");
            options.Facets.Add("Folders");

            // Handle sorting
            if (!string.IsNullOrWhiteSpace(sort))
            {
                switch (sort.ToLower())
                {
                    case "title_asc":
                        options.OrderBy.Add("Title asc");
                        break;
                    case "title_desc":
                        options.OrderBy.Add("Title desc");
                        break;
                    case "publishdate_asc":
                        options.OrderBy.Add("PublishSince asc");
                        break;
                    case "publishdate_desc":
                        options.OrderBy.Add("PublishSince desc");
                        break;
                }
            }

            // Execute search query
            var results = await _searchClient.SearchAsync<ArticleSearchModel>(query, options);

            // If there are no results or no hits, return empty result set
            if (results.Value == null || results.Value.TotalCount == 0)
            {
                return null;
            }

            // Return search results
            return results.Value;
        }

        /// <summary>
        /// Get just get facets of channel.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="isArticle"></param>
        /// <param name="isArchived"></param>
        /// <param name="showAuthor"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, IList<FacetResult>>> GetFacetsAsync(
    string partitionKey,
    bool? isArticle = null,
    bool? isArchived = null,
    bool? showAuthor = null)
        {
            var filterClauses = new List<string>
    {
        $"PartitionKey eq '{partitionKey}'"
    };

            if (isArticle.HasValue) filterClauses.Add($"IsArticle eq {isArticle.Value.ToString().ToLower()}");
            if (isArchived.HasValue) filterClauses.Add($"IsArchived eq {isArchived.Value.ToString().ToLower()}");
            if (showAuthor.HasValue) filterClauses.Add($"ShowAuthor eq {showAuthor.Value.ToString().ToLower()}");

            var filter = string.Join(" and ", filterClauses);

            var options = new SearchOptions
            {
                Filter = filter,
                Size = 0, // Set size to 0 as we are not interested in documents
            };

            options.Facets.Add("Tags");
            options.Facets.Add("Folders");

            var results = await _searchClient.SearchAsync<ArticleSearchModel>("*", options); // Use wildcard to fetch facets
            return results.Value.Facets;
        }

        /// <summary>
        /// Get related articles.
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="currentArticleId"></param>
        /// <param name="tags"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<SearchResults<ArticleSearchModel>?> SearchRelatedArticlesAsync(
    string partitionKey,
    string currentArticleId,
    IEnumerable<string> tags,
    string culture,
    int pageSize = 5)
        {
            if (tags == null || !tags.Any())
            {
                return null; // Indicate no related articles were found
            }

            var filterClauses = new List<string>
            {
                $"PartitionKey eq '{partitionKey}'",
                $"RowKey ne '{currentArticleId}'" // Exclude the current article
            };

            filterClauses.Add($"PublishSince le {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            filterClauses.Add($"PublishUntil ge {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
            filterClauses.Add($"Culture eq '{culture}'");

            // Match any of the tags
            var tagFilters = tags.Select(tag => $"Tags/any(t: t eq '{tag}')");
            filterClauses.Add($"({string.Join(" or ", tagFilters)})");

            var filter = string.Join(" and ", filterClauses);

            var options = new SearchOptions
            {
                Filter = filter,
                Size = pageSize,
                IncludeTotalCount = true
            };

            options.Select.Add("Title");
            options.Select.Add("PublishSince");
            options.Select.Add("Culture");
            options.Select.Add("PermaName");
            options.Select.Add("Tags");
            options.Select.Add("MainImageUrl");

            var results = await _searchClient.SearchAsync<ArticleSearchModel>("*", options);
            return results.Value;
        }

        public async Task IndexArticlesAsync(IEnumerable<ArticleSearchModel> articles)
        {
            await _searchClient.UploadDocumentsAsync(articles);
            Console.WriteLine("Documents uploaded to the search index.");
        }

        public async Task UnIndexArticlesAsync(IEnumerable<string> articleIds)
        {
            // Create objects with only the RowKey for deletion
            var keysToDelete = articleIds.Select(id => new { RowKey = id }).ToList();

            // Use DeleteDocumentsAsync with the minimal data required
            await _searchClient.DeleteDocumentsAsync(keysToDelete);
            Console.WriteLine("Documents deleted from the search index.");
        }

        #endregion
    }
}
