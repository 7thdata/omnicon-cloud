using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.Extensions.Options;

namespace clsCms.Services
{
    public class BlobStorageServices : IBlobStorageServices
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IOptions<AppConfigModel> _appConfig;

        /// <summary>
        /// Construct BlobStorageServices
        /// </summary>
        /// <param name="configuration"></param>
        public BlobStorageServices(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig;
            _blobServiceClient = new BlobServiceClient(_appConfig.Value.AzureStorage.BlobConnectionString);
        }

        /// <summary>
        /// Upload image with thumbnail.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="fileStream"></param>
        /// <param name="originalFileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<(string originalUrl, string thumbnailUrl)> UploadImageWithThumbnailAsync(string containerName, Stream fileStream, string originalFileName, string contentType)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var fileId = Guid.NewGuid().ToString();

            // Generate a new GUID for the file name
            var fileExtension = Path.GetExtension(originalFileName);
            var newFileName = $"{fileId}{fileExtension}";

            // Upload original image with the new GUID file name
            var blobClient = blobContainerClient.GetBlobClient(newFileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(fileStream, blobHttpHeaders);
            var originalUrl = blobClient.Uri.ToString();

            // Generate a new GUID for the thumbnail file name
            var thumbnailFileName = $"{fileId}-thumbnail{fileExtension}";
            var thumbnailStream = await GenerateThumbnailAsync(fileStream);

            // Upload thumbnail with the new GUID file name
            var thumbnailBlobClient = blobContainerClient.GetBlobClient(thumbnailFileName);
            await thumbnailBlobClient.UploadAsync(thumbnailStream, new BlobHttpHeaders { ContentType = contentType });
            var thumbnailUrl = thumbnailBlobClient.Uri.ToString();

            return (originalUrl, thumbnailUrl);
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string containerName)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = blobContainerClient.GetBlobClient(fileName);
                var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
                await blobClient.UploadAsync(fileStream, blobHttpHeaders);

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error uploading file to Azure Blob Storage: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string fileName, string containerName)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting file from Azure Blob Storage: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public string GetFileUrl(string fileName, string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Generates a thumbnail from the image stream.
        /// </summary>
        /// <param name="imageStream">Original image stream</param>
        /// <returns>Stream of the generated thumbnail</returns>
        private async Task<Stream> GenerateThumbnailAsync(Stream imageStream)
        {
            imageStream.Position = 0; // Reset stream position

            using var image = await Image.LoadAsync(imageStream);
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(150, 150),
                Mode = ResizeMode.Crop
            }));

            var thumbnailStream = new MemoryStream();
            await image.SaveAsJpegAsync(thumbnailStream);
            thumbnailStream.Position = 0; // Reset stream position for upload

            return thumbnailStream;
        }

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
        public async Task<(List<FileMetadataModel> files, string continuationToken)> GetListFilesAsync(
     string containerName,
     string? continuationToken = null,
     int pageSize = 10,
     string? nameFilter = null,
     string? typeFilter = null,
     long? minSize = null,
     long? maxSize = null)
        {
            try
            {
                // Get the BlobContainerClient
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure the container exists (create it if not)
                await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

                var files = new List<FileMetadataModel>();

                // List blobs with pagination
                var resultSegment = blobContainerClient.GetBlobsAsync().AsPages(continuationToken, pageSize);

                await foreach (var blobPage in resultSegment)
                {
                    foreach (var blobItem in blobPage.Values)
                    {
                        var size = blobItem.Properties.ContentLength ?? 0;
                        var contentType = blobItem.Properties.ContentType;

                        // Apply filters
                        if (!string.IsNullOrEmpty(nameFilter) && !blobItem.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (!string.IsNullOrEmpty(typeFilter) && !string.Equals(contentType, typeFilter, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (minSize.HasValue && size < minSize.Value)
                            continue;

                        if (maxSize.HasValue && size > maxSize.Value)
                            continue;

                        // Create a BlobClient for generating the file URL
                        var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);

                        // Create a FileMetadata object for each file
                        var fileMetadata = new FileMetadataModel
                        {
                            Name = blobItem.Name,
                            Size = size,
                            ContentType = contentType,
                            LastModified = blobItem.Properties.LastModified?.DateTime,
                            ETag = blobItem.Properties.ETag?.ToString(),
                            Url = blobClient.Uri.ToString() // Get the URL of the blob
                        };

                        files.Add(fileMetadata);
                    }

                    // Return the continuation token for the next page
                    continuationToken = blobPage.ContinuationToken;

                    // Stop after the first page if only one page is needed
                    break;
                }

                return (files, continuationToken);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error listing files in Azure Blob Storage: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<FileMetadataModel?> GetFileMetadataAsync(string containerName, string fileName)
        {
            try
            {
                // Get the BlobContainerClient
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Get the BlobClient for the specific file
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                // Check if the blob exists
                if (await blobClient.ExistsAsync())
                {
                    // Fetch blob properties
                    var properties = await blobClient.GetPropertiesAsync();

                    // Create and return the FileMetadataModel
                    return new FileMetadataModel
                    {
                        Name = fileName,
                        Size = properties.Value.ContentLength,
                        ContentType = properties.Value.ContentType,
                        LastModified = properties.Value.LastModified.DateTime,
                        ETag = properties.Value.ETag.ToString(),
                        Url = blobClient.Uri.ToString()
                    };
                }
                else
                {
                    // Return null if the file does not exist
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving metadata for file '{fileName}' in container '{containerName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves the URL for the thumbnail of a specific file.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is stored.</param>
        /// <param name="fileName">The name of the original file.</param>
        /// <returns>The URL of the thumbnail, or null if the thumbnail does not exist.</returns>
        public async Task<string?> GetThumbnailUrlAsync(string containerName, string fileName)
        {
            try
            {
                // Get the BlobContainerClient
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Derive the thumbnail file name based on naming convention
                var fileExtension = Path.GetExtension(fileName);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var thumbnailFileName = $"{fileNameWithoutExtension}-thumbnail{fileExtension}";

                // Get the BlobClient for the thumbnail
                var thumbnailBlobClient = blobContainerClient.GetBlobClient(thumbnailFileName);

                // Check if the thumbnail exists
                if (await thumbnailBlobClient.ExistsAsync())
                {
                    // Return the URL of the thumbnail
                    return thumbnailBlobClient.Uri.ToString();
                }

                // Return null if the thumbnail does not exist
                return null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error retrieving thumbnail URL for file '{fileName}' in container '{containerName}': {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Edits an existing file in Azure Blob Storage.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="currentFileName"></param>
        /// <param name="newFileName"></param>
        /// <param name="newFileStream"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task<string> EditFileAsync(
     string containerName,
     string currentFileName,
     string? newFileName = null,
     Stream? newFileStream = null,
     string? contentType = null)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Get the BlobClient for the current file
                var currentBlobClient = blobContainerClient.GetBlobClient(currentFileName);

                // Ensure the file exists
                if (!await currentBlobClient.ExistsAsync())
                {
                    throw new FileNotFoundException($"File '{currentFileName}' does not exist in container '{containerName}'.");
                }

                // Determine the target file name
                var targetFileName = string.IsNullOrWhiteSpace(newFileName) ? currentFileName : newFileName;

                // If renaming is required
                if (!string.Equals(currentFileName, targetFileName, StringComparison.OrdinalIgnoreCase))
                {
                    var targetBlobClient = blobContainerClient.GetBlobClient(targetFileName);
                    await targetBlobClient.StartCopyFromUriAsync(currentBlobClient.Uri);

                    var properties = await targetBlobClient.GetPropertiesAsync();
                    while (properties.Value.CopyStatus == CopyStatus.Pending)
                    {
                        await Task.Delay(500);
                        properties = await targetBlobClient.GetPropertiesAsync();
                    }

                    if (properties.Value.CopyStatus != CopyStatus.Success)
                    {
                        throw new ApplicationException($"Failed to copy blob '{currentFileName}' to '{targetFileName}'.");
                    }

                    await currentBlobClient.DeleteIfExistsAsync();
                }

                // If replacing the file content
                if (newFileStream != null)
                {
                    var targetBlobClient = blobContainerClient.GetBlobClient(targetFileName);
                    var headers = new BlobHttpHeaders();

                    if (!string.IsNullOrWhiteSpace(contentType))
                    {
                        headers.ContentType = contentType;
                    }

                    await targetBlobClient.UploadAsync(newFileStream, new BlobUploadOptions
                    {
                        HttpHeaders = headers
                    });
                }

                return blobContainerClient.GetBlobClient(targetFileName).Uri.ToString();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error editing file '{currentFileName}' in container '{containerName}': {ex.Message}", ex);
            }
        }
    }


}
