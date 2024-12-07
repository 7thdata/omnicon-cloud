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

namespace clsCms.Services
{
    public class BlobStorageServices : IBlobStorageServices
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public BlobStorageServices(IConfiguration configuration)
        {
            var connectionString = configuration["AppSettings:AzureStorage:BlobConnectionString"];
            _containerName = configuration["AppSettings:AzureStorage:BlobContainerName"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        /// <inheritdoc/>
        public async Task<(string originalUrl, string thumbnailUrl)> UploadImageWithThumbnailAsync(Stream fileStream, string originalFileName, string contentType)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Generate a new GUID for the file name
            var fileExtension = Path.GetExtension(originalFileName);
            var newFileName = $"{Guid.NewGuid()}{fileExtension}";

            // Upload original image with the new GUID file name
            var blobClient = blobContainerClient.GetBlobClient(newFileName);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = contentType };
            await blobClient.UploadAsync(fileStream, blobHttpHeaders);
            var originalUrl = blobClient.Uri.ToString();

            // Generate a new GUID for the thumbnail file name
            var thumbnailFileName = $"{Guid.NewGuid()}-thumbnail{fileExtension}";
            var thumbnailStream = await GenerateThumbnailAsync(fileStream);

            // Upload thumbnail with the new GUID file name
            var thumbnailBlobClient = blobContainerClient.GetBlobClient(thumbnailFileName);
            await thumbnailBlobClient.UploadAsync(thumbnailStream, new BlobHttpHeaders { ContentType = contentType });
            var thumbnailUrl = thumbnailBlobClient.Uri.ToString();

            return (originalUrl, thumbnailUrl);
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
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
        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting file from Azure Blob Storage: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public string GetFileUrl(string fileName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
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
    }
}
