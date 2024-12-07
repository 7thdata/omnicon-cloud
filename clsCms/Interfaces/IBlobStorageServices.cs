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
        /// <returns>A tuple containing URLs of the original image and the thumbnail.</returns>
        Task<(string originalUrl, string thumbnailUrl)> UploadImageWithThumbnailAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Uploads a generic file to Azure Blob Storage.
        /// </summary>
        /// <param name="fileStream">The stream of the file to upload.</param>
        /// <param name="fileName">The name of the file to be stored in the blob.</param>
        /// <param name="contentType">The MIME type of the file.</param>
        /// <returns>The URL of the uploaded file.</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Deletes a file from Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to delete.</param>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// Gets the URL of a file in Azure Blob Storage.
        /// </summary>
        /// <param name="fileName">The name of the file to retrieve the URL for.</param>
        /// <returns>The URL of the file.</returns>
        string GetFileUrl(string fileName);
    }
}
