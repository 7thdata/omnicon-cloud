using clsCms.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface IBlobStorageServices
    {
        /// <summary>
        /// Uploads an image file and creates a thumbnail version.
        /// </summary>
        /// <param name="fileStream">The stream of the image file to upload.</param>
        /// <param name="fileName">The name of the file to be stored in the blob.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <param name="containerName">The name of the container to upload the file to.</param>
        /// <returns>A tuple containing URLs of the original image and the thumbnail.</returns>
        Task<(string originalUrl, string thumbnailUrl)> UploadImageWithThumbnailAsync(string containerName, Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Uploads a generic file to Azure Blob Storage.
        /// </summary>
        /// <param name="fileStream">The stream of the file to upload.</param>
        /// <param name="fileName">The name of the file to be stored in the blob.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <param name="containerName">The name of the container to upload the file to.</param>
        /// <returns>The URL of the uploaded file.</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName);

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <param name="containerName">The name of the container where the file is stored.</param>
        Task DeleteFileAsync(string fileName, string containerName);

        /// <summary>
        /// Gets the URL of a file in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve the URL for.</param>
        /// <param name="containerName">The name of the container where the file is stored.</param>
        /// <returns>The URL of the file.</returns>
        string GetFileUrl(string fileName, string containerName);

        /// <summary>
        /// Lists and filters files in the specified container with optional pagination and filters.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="continuationToken">Token for fetching the next set of results.</param>
        /// <param name="pageSize">Number of results to fetch per page (optional).</param>
        /// <param name="nameFilter">Filter files by name (optional).</param>
        /// <param name="typeFilter">Filter files by content type (optional).</param>
        /// <param name="minSize">Minimum file size in bytes (optional).</param>
        /// <param name="maxSize">Maximum file size in bytes (optional).</param>
        /// <returns>A tuple containing the filtered list of file objects and the continuation token.</returns>
        Task<(List<FileMetadataModel> files, string continuationToken)> GetListFilesAsync(
            string containerName,
            string? continuationToken = null,
            int pageSize = 10,
            string? nameFilter = null,
            string? typeFilter = null,
            long? minSize = null,
            long? maxSize = null);

        /// <inheritdoc/>
        Task<FileMetadataModel?> GetFileMetadataAsync(string containerName, string fileName);

        /// <summary>
        /// Retrieves the URL for the thumbnail of a specific file.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is stored.</param>
        /// <param name="fileName">The name of the original file.</param>
        /// <returns>The URL of the thumbnail, or null if the thumbnail does not exist.</returns>
        Task<string?> GetThumbnailUrlAsync(string containerName, string fileName);

       
        /// <summary>
        /// Edit file.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="currentFileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="newFileStream"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Task<string> EditFileAsync(
     string containerName,
     string currentFileName,
     string? newFileName = null,
     Stream? newFileStream = null,
     string? contentType = null);
    }
}
