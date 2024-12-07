using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using clsCms.Models;

namespace clsCms.Interfaces
{
    /// <summary>
    /// Provides services related to authors.
    /// </summary>
    public interface IAuthorServices
    {
        /// <summary>
        /// Creates a new author entry in the table storage.
        /// </summary>
        /// <param name="author">The author model to create.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task CreateAuthorAsync(AuthorModel author);

        /// <summary>
        /// Reads (retrieves) an author by ChannelId and AuthorId (PartitionKey and RowKey).
        /// </summary>
        /// <param name="channelId">The channel ID (PartitionKey).</param>
        /// <param name="authorId">The author ID (RowKey).</param>
        /// <returns>The AuthorModel if found, otherwise null.</returns>
        Task<AuthorModel> GetAuthorAsync(string channelId, string authorId);

        /// <summary>
        /// Updates an existing author entry in the table storage.
        /// </summary>
        /// <param name="author">The author model to update.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task UpdateAuthorAsync(AuthorModel author);

        /// <summary>
        /// Deletes (archives) an author by setting the IsArchived flag to true.
        /// </summary>
        /// <param name="channelId">The channel ID (PartitionKey).</param>
        /// <param name="authorId">The author ID (RowKey).</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        Task DeleteAuthorAsync(string channelId, string authorId);

        /// <summary>
        /// Lists all authors for a specific channel, optionally filtering out archived authors.
        /// </summary>
        /// <param name="channelId">The channel ID (PartitionKey).</param>
        /// <param name="includeArchived">Whether to include archived authors.</param>
        /// <returns>A list of AuthorModel objects.</returns>
        Task<List<AuthorModel>> ListAuthorsByChannelAsync(string channelId, bool includeArchived = false);

        /// <summary>
        /// /Get list of authors
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="includeArchived"></param>
        /// <returns></returns>
        Task<List<AuthorModel>> ListAuthorsByUserIdAndOrganizationIdAsync(string organizationId, string userId, bool includeArchived = false);


        /// <summary>
        /// Gets a randomly selected profile image path.
        /// </summary>
        /// <returns>The relative file path of a randomly selected profile image.</returns>
        string GetRandomProfileImage();
    }
}
