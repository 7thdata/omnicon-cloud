using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using clsCms.Helpers;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.Extensions.Options;

namespace clsCms.Services
{
    /// <summary>
    /// Provides services related to authors.
    /// </summary>
    public class AuthorServices : IAuthorServices
    {
        private readonly TableClient _authorTable;
        private readonly AppConfigModel _appConfig;
        private const string AuthorTableName = "AuthorsTable";
        private readonly IChannelServices _channelServices;

        public AuthorServices(IOptions<AppConfigModel> appConfig, IChannelServices channelServices)
        {
            _appConfig = appConfig.Value;

            var serviceClient = new TableServiceClient(
                new Uri(_appConfig.AzureStorage.StorageTableUri),
                new TableSharedKeyCredential(
                    _appConfig.AzureStorage.StorageAccountName,
                    _appConfig.AzureStorage.StorageAccountKey
                ));

            _authorTable = serviceClient.GetTableClient(AuthorTableName);
            _authorTable.CreateIfNotExists();
            _channelServices = channelServices;
        }

        /// <summary>
        /// Creates a new author entry in the table storage.
        /// </summary>
        /// <param name="author">The author model to create.</param>
        public async Task CreateAuthorAsync(AuthorModel author)
        {
            author.RowKey = Guid.NewGuid().ToString();
            author.PartitionKey = author.ChannelId;
            await _authorTable.AddEntityAsync(author);
        }

        /// <summary>
        /// Reads (retrieves) an author by ChannelId and AuthorId (PartitionKey and RowKey).
        /// </summary>
        /// <param name="channelId">The channel ID (PartitionKey).</param>
        /// <param name="authorId">The author ID (RowKey).</param>
        /// <returns>The AuthorModel if found, otherwise null.</returns>
        public async Task<AuthorModel> GetAuthorAsync(string channelId, string authorId)
        {
            try
            {
                var response = await _authorTable.GetEntityAsync<AuthorModel>(channelId, authorId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Handle the case where the author is not found
            }
        }

        /// <summary>
        /// Updates an existing author entry in the table storage.
        /// </summary>
        /// <param name="author">The author model to update.</param>
        public async Task UpdateAuthorAsync(AuthorModel author)
        {
            try
            {
                await _authorTable.UpdateEntityAsync(author, author.ETag, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                throw new KeyNotFoundException($"Author with ID {author.RowKey} not found.");
            }
        }

        /// <summary>
        /// Deletes (archives) an author by setting the IsArchived flag to true.
        /// </summary>
        /// <param name="channelId">The channel ID (PartitionKey).</param>
        /// <param name="authorId">The author ID (RowKey).</param>
        public async Task DeleteAuthorAsync(string channelId, string authorId)
        {
            var author = await GetAuthorAsync(channelId, authorId);

            if (author == null)
            {
                throw new KeyNotFoundException($"Author with ID {authorId} not found.");
            }

            // Soft delete (archive) by setting IsArchived to true
            author.IsArchived = true;
            await UpdateAuthorAsync(author);
        }

        /// <summary>
        /// Lists all authors for a specific channel, optionally filtering out archived authors.
        /// </summary>
        /// <param name="channelId">The channel ID (PartitionKey).</param>
        /// <param name="includeArchived">Whether to include archived authors.</param>
        /// <returns>A list of AuthorModel objects.</returns>
        public async Task<List<AuthorModel>> ListAuthorsByChannelAsync(string channelId, bool includeArchived = false)
        {
            var authors = new List<AuthorModel>();

            await foreach (var author in _authorTable.QueryAsync<AuthorModel>(a => a.PartitionKey == channelId))
            {
                if (includeArchived || !author.IsArchived)
                {
                    authors.Add(author);
                }
            }

            return authors;
        }

        /// <summary>
        /// /Get list of authors
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="includeArchived"></param>
        /// <returns></returns>
        public async Task<List<AuthorModel>> ListAuthorsByUserIdAndOrganizationIdAsync(string organizationId, string userId, bool includeArchived = false)
        {
            // Fetch all channels associated with the user. 
            // Pagination is set to get up to 1000 channels for the user.
            var channels = await _channelServices.GetChannelsByUserIdAndOrganizationIdAsync(organizationId, userId, "", "", 1, 1000);

            // Initialize a list to hold the authors retrieved from the channels.
            var authors = new List<AuthorModel>();

            // Loop through each channel the user is associated with.
            foreach (var channel in channels.Items)
            {
                // Query the authors table for authors associated with the channel (PartitionKey == channel ID)
                await foreach (var author in _authorTable.QueryAsync<AuthorModel>(a => a.PartitionKey == channel.Channel.Id))
                {
                    // If includeArchived is true, add all authors.
                    // Otherwise, only add authors that are not archived.
                    if (includeArchived || !author.IsArchived)
                    {
                        authors.Add(author);
                    }
                }
            }

            // Return the list of authors.
            return authors;
        }


        /// <summary>
        /// Gets a randomly selected profile image path.
        /// </summary>
        /// <returns>The relative file path of a randomly selected profile image.</returns>
        public string GetRandomProfileImage()
        {
            return FileSelectionHelper.GetRandomProfileImage();
        }
    }
}
