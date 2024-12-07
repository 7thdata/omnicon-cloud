using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using clsCms.Models;
using clsCMs.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Azure.Core;
using clsCms.Interfaces;
using Microsoft.Extensions.Primitives;

namespace clsCms.Services
{

    public class ArticleServices : IArticleServices
    {
        private readonly TableClient _folderTable;
        private readonly TableClient _articleTable;
        private readonly AppConfigModel _appConfig;
        private readonly ApplicationDbContext _db;
        private readonly IChannelServices _channelServices;

        private readonly IAuthorServices _authorServices;

        // Hardcoded table names
        private const string FolderTableName = "FoldersTable";
        private const string ArticleTableName = "ArticlesTable";

        // Simplified constructor with AppConfigModel using IOptions
        public ArticleServices(IOptions<AppConfigModel> appConfig, 
            IChannelServices channelServices, ApplicationDbContext db,
            IAuthorServices authorServices)
        {
            _appConfig = appConfig.Value;

            var serviceClient = new TableServiceClient(
                new Uri(_appConfig.AzureStorage.StorageTableUri),
                new TableSharedKeyCredential(
                    _appConfig.AzureStorage.StorageAccountName,
                    _appConfig.AzureStorage.StorageAccountKey
                )
            );

            _channelServices = channelServices;

            _folderTable = serviceClient.GetTableClient(FolderTableName);
            _articleTable = serviceClient.GetTableClient(ArticleTableName);

            _folderTable.CreateIfNotExists();
            _articleTable.CreateIfNotExists();
            _db = db;
            _authorServices = authorServices;
        }

        #region Folder CRUD

        public async Task CreateFolderAsync(FolderModel folder)
        {
            folder.RowKey = Guid.NewGuid().ToString();
            await _folderTable.AddEntityAsync(folder);
        }

        public async Task<FolderModel> GetFolderAsync(string folderId)
        {
            return await _folderTable.GetEntityAsync<FolderModel>("Folders", folderId);
        }

        public async Task UpdateFolderAsync(FolderModel folder)
        {
            await _folderTable.UpdateEntityAsync(folder, folder.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteFolderAsync(string folderId)
        {
            await _folderTable.DeleteEntityAsync("Folders", folderId);
        }

        public async Task<List<FolderModel>> ListFoldersAsync(string channelId)
        {
            return _folderTable.Query<FolderModel>(f => f.ChannelId == channelId && !f.IsArchived).ToList();
        }

        #endregion

        #region Article CRUD

        public async Task<List<ArticleModel>> GetArticlesByChannelIdAsync(string userId, string channelId)
        {
            var articles = new List<ArticleModel>();

            // Query the table and iterate over the results
            await foreach (var article in _articleTable.QueryAsync<ArticleModel>(a => a.PartitionKey == channelId && a.ChannelId == channelId))
            {
                articles.Add(article);
            }

            return articles;
        }

        public async Task<PaginationModel<ArticleModel>> SearchArticlesAsync(
     string channelId,
     string searchQuery,
     int currentPage,
     int itemsPerPage,
     string sort = null)
        {
            var articles = new List<ArticleModel>();
            string[]? searchTerms = null;

            // Split search query into terms if provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchTerms = searchQuery.ToLower().Split(' ');
            }

            // Base query to fetch articles by ChannelId and IsArchived flag
            await foreach (var article in _articleTable.QueryAsync<ArticleModel>(a =>
                a.PartitionKey == channelId && a.IsArchived == false))
            {
                articles.Add(article);
            }

            // Apply search filters in-memory if search terms are provided
            var filteredArticles = searchTerms != null
                ? articles.Where(a =>
                    searchTerms.All(term =>
                        (a.Title != null && a.Title.ToLower().Contains(term)) ||
                        (a.Text != null && a.Text.ToLower().Contains(term)) ||
                        (a.Description != null && a.Description.ToLower().Contains(term)) ||
                        (a.Tags != null && a.Tags.ToLower().Contains(term)) ||
                        (a.AuthorName != null && a.AuthorName.ToLower().Contains(term)) ||
                        (a.PermaName != null && a.PermaName.ToLower().Contains(term))))
                    .ToList()
                : articles;

            // Apply sorting if specified
            if (!string.IsNullOrEmpty(sort))
            {
                filteredArticles = sort.ToLower() switch
                {
                    "title" => filteredArticles.OrderBy(a => a.Title).ToList(),
                    "publishdate" => filteredArticles.OrderByDescending(a => a.PublishSince).ToList(),
                    _ => filteredArticles
                };
            }

            // Pagination setup
            int totalItems = filteredArticles.Count;
            int totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
            var paginatedItems = filteredArticles
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            // Wrap the result in PaginationModel
            return new PaginationModel<ArticleModel>(
                items: paginatedItems,
                currentPage: currentPage,
                itemsPerPage: itemsPerPage,
                totalItems: totalItems,
                totalPages: totalPages)
            {
                Keyword = searchQuery,
                Sort = sort
            };
        }

        public async Task CreateArticleAsync(ArticleModel article)
        {
            await _articleTable.AddEntityAsync(article);

            // Count up
            var countResult = await _channelServices.AddChannelAricleCountAsync(article.ChannelId, 1);
        }

        public async Task<ArticleModel?> GetArticleByChannelIdAndIdAsync(string channelId, string articleId)
        {
            // Query the table for the specific article by PartitionKey (userId) and RowKey (articleId)
            var query = _articleTable.QueryAsync<ArticleModel>(
                a => a.PartitionKey == channelId && a.RowKey == articleId);

            // Retrieve the first matching article, if any
            await foreach (var article in query)
            {
                return article; // Return the first matching article
            }

            // Return null if no article is found
            return null;
        }

        public async Task<bool> IsArticlePermaNameUniqueAsync(string userId, string permaName)
        {
            // Fetch all results matching the partition key (userId) and permaName
            await foreach (var article in _articleTable.QueryAsync<ArticleModel>(a => a.PartitionKey == userId && a.PermaName == permaName))
            {
                // If any article matches, return false (not unique)
                if (article != null)
                {
                    return false;
                }
            }

            // If no matching article is found, return true (PermaName is unique)
            return true;
        }

        public async Task<ArticleModel> GetArticleAsync(string articleId)
        {
            return await _articleTable.GetEntityAsync<ArticleModel>("Articles", articleId);
        }

        /// <summary>
        /// Get article view.
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public async Task<ArticleViewModel> GetArticleViewAsync(string channelId, string articleId)
        {
            // Query the table for the specific article by PartitionKey (userId) and RowKey (articleId)
            var query = _articleTable.QueryAsync<ArticleModel>(
                a => a.PartitionKey == channelId && a.RowKey == articleId);

            // Retrieve the first matching article
            ArticleModel article = null;
            await foreach (var item in query)
            {
                article = item;
                break; // Exit after getting the first item
            }

            if (article == null)
            {
                throw new Exception($"Article with ID '{articleId}' in channel '{channelId}' not found.");
            }

            // Fetch related data
            var author = await _authorServices.GetAuthorAsync(article.ChannelId, article.AuthorId);
            var channel = await _channelServices.GetChannelAsync(article.ChannelId);

            // Map the result to the ViewModel
            var result = new ArticleViewModel()
            {
                Article = article,
                Author = author,
                Channel = channel
            };

            return result;
        }

        /// <summary>
        /// Get article view from permaName.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="permaName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ArticleViewModel?> GetArticleViewByPermaNameAsync(string channelId, string permaName)
        {
            // Query the table for the specific article by PartitionKey (userId) and RowKey (articleId)
            var query = _articleTable.QueryAsync<ArticleModel>(
                a => a.PartitionKey == channelId && a.PermaName == permaName);

            // Retrieve the first matching article
            ArticleModel article = null;
            await foreach (var item in query)
            {
                article = item;
                break; // Exit after getting the first item
            }

            if (article == null)
            {
                throw new Exception($"Article with permaName '{permaName}' in channel '{channelId}' not found.");
            }

            // Fetch related data
            var author = await _authorServices.GetAuthorAsync(article.ChannelId, article.AuthorId);
            var channel = await _channelServices.GetChannelAsync(article.ChannelId);

            // Map the result to the ViewModel
            var result = new ArticleViewModel()
            {
                Article = article,
                Author = author,
                Channel = channel
            };

            return result;
        }

        public async Task UpdateArticleAsync(ArticleModel article)
        {
            article.Timestamp = DateTime.UtcNow;
            await _articleTable.UpdateEntityAsync(article, article.ETag, TableUpdateMode.Replace);
        }

        public async Task DeleteArticleAsync(string channelId, string rowKey)
        {
            var partitionKey = channelId; // Assuming channelId is used as partition key
            await _articleTable.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<List<ArticleModel>> ListArticlesAsync(string channelId, List<string> folders)
        {
            return _articleTable.Query<ArticleModel>(a => a.ChannelId == channelId && !a.IsArchived && folders.Any(f => a.Folders.Contains(f))).ToList();
        }

        #endregion

        #region Article Impressions

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
        public async Task<ArticleImpressionModel> UpsertClickImpressionAsync(string articleId,
            string channelId, string folderId, string authorId, string culture,
            string tags, string userAgent, string browser, string ipAddress, string referrer,
            string userId, string deviceId, string os, string country, string city, string language)
        {
            var impression = new ArticleImpressionModel()
            {
                ArticleId = articleId,
                ChannelId = channelId,
                FolderId = folderId,
                AuthorId = authorId,
                Culture = culture,
                Tags = tags,
                ImpressionId = Guid.NewGuid().ToString(),
                UserAgent = userAgent,
                Referrer = referrer,
                UserId = userId,
                DeviceId = deviceId,
                IpAddress = ipAddress,
                ImpressionTime = DateTimeOffset.UtcNow,
                Browser = browser,
                Os = os,
                Country = country,
                City = city,
                Language = language
            };

            _db.ArticleImpressions.Add(impression);
            await _db.SaveChangesAsync();

            return impression;
        }

        /// <summary>
        /// Get contents of stack barchart for the top page.
        /// </summary>
        /// <param name="organizationId"
        /// <param name="since"></param>
        /// <param name="until"></param>
        /// <returns></returns>
        public List<ArticleImpressionByDayViewModel> GetDailyImpressions(string organizationId, DateTimeOffset since, DateTimeOffset until)
        {
            var impressions = _db.ArticleImpressions
                .Where(i => i.ImpressionTime >= since && i.ImpressionTime <= until && i.OrganizationId == organizationId)
                .GroupBy(i => new { i.ImpressionTime.Date, i.ChannelId })
                .Select(g => new
                {
                    Date = g.Key.Date,
                    ChannelId = g.Key.ChannelId,
                    Impressions = g.Count()
                })
                .ToList();

            var viewModel = impressions
                .GroupBy(i => i.Date)
                .Select(g => new ArticleImpressionByDayViewModel
                {
                    DateTime = g.Key,
                    Impressions = g.Select(impression => new ArticleImpressionStatsViewModel
                    {
                        ChannelId = impression.ChannelId,
                        Impressions = impression.Impressions
                    }).ToList()
                })
                .ToList();

            return viewModel;
        }
        #endregion
    }
}
