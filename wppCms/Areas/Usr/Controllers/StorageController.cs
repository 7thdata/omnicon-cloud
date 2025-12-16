using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class StorageController : UsrBaseController
    {
        private readonly AppConfigModel _appConfig;
        private readonly IBlobStorageServices _blobStorageServices;

        public StorageController(UserManager<UserModel> userManager,
        IChannelServices channelServices, IOptions<AppConfigModel> appConfig,
            IBlobStorageServices blobStorageServices) : base(userManager, channelServices)
        {
            _appConfig = appConfig.Value;
            _blobStorageServices = blobStorageServices;
        }

        [Route("/{culture}/usr/channel/{channelId}/storage")]
        public async Task<IActionResult> Index(string culture, string channelId, string? continuationToken,
            string? nameFilter, string? typeFilter, long? minSize, long? maxSize, int pageSize = 10)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            // Get list of medias
            var listOfFiles = await _blobStorageServices.GetListFilesAsync(channel.Channel.Id,
                continuationToken, pageSize, nameFilter, typeFilter, minSize, maxSize);

            var view = new UsrStorageIndexViewModel()
            {
                Culture = culture,
                Channel = channel,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Files = listOfFiles.files,
                ContinuationToken = listOfFiles.continuationToken,
                NameFilter = nameFilter,
                TypeFilter = typeFilter,
                MinSize = minSize,
                MaxSize = maxSize,
            };

            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/storage/upload")]
        public async Task<IActionResult> UploadFile(string culture, string channelId, string? folderName, IFormFile file)
        {
            try
            {
                // Validate input
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                // Get the current user and channel
                var user = await GetAuthenticatedUserAsync();
                var channel = await GetChannelAsync(channelId);

                // Use the channel ID as the container name
                var containerName = channel.Channel.Id;

                // Generate the blob name, optionally including the folder name
                var blobName = string.IsNullOrWhiteSpace(folderName)
                    ? $"{Guid.NewGuid()}_{file.FileName}" // Store directly in the container
                    : $"{folderName.Trim().Replace(" ", "-")}/{Guid.NewGuid()}_{file.FileName}"; // Store in the folder

                // Check if the file is an image
                var isImage = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

                // Process the upload based on the file type
                using (var fileStream = file.OpenReadStream())
                {
                    if (isImage)
                    {
                        // Call the image-specific upload method
                        var result = await _blobStorageServices.UploadImageWithThumbnailAsync(
                            containerName: containerName,
                            fileStream: fileStream,
                            fileName: blobName,
                            contentType: file.ContentType
                        );

                        // Return success response with URLs
                        return Json(new
                        {
                            success = true,
                            originalUrl = result.originalUrl,
                            thumbnailUrl = result.thumbnailUrl
                        });
                    }
                    else
                    {
                        // Call the general file upload method
                        var fileUrl = await _blobStorageServices.UploadFileAsync(
                            fileStream: fileStream,
                            fileName: blobName,
                            contentType: file.ContentType,
                            containerName: containerName
                        );

                        // Return success response with the file URL
                        return Json(new
                        {
                            success = true,
                            fileUrl = fileUrl
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error (optional)
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet]
        [Route("/{culture}/usr/channel/{channelId}/storage/details/{fileName}")]
        public async Task<IActionResult> Details(string culture, string channelId, string fileName)
        {
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var file = await _blobStorageServices.GetFileMetadataAsync(channel.Channel.Id, fileName);

            if (file == null)
                return NotFound();

            var viewModel = new UsrStorageDetailsViewModel
            {
                Culture = culture,
                Channel = channel,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                File = file,
                ThumbnailUrl = file.ContentType.StartsWith("image/") ? await _blobStorageServices.GetThumbnailUrlAsync(channel.Channel.Id, fileName) : null
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/storage/edit/{fileName}")]
        public async Task<IActionResult> Edit(
     string culture,
     string channelId,
     string fileName,
     [FromForm] string newFileName,
     [FromForm] IFormFile newFile)
        {
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            // Open the uploaded file as a stream
            using var stream = newFile.OpenReadStream();

            // Call the updated service method with stream and content type
            await _blobStorageServices.EditFileAsync(
                channel.Channel.Id,
                fileName,
                newFileName,
                stream,
                newFile.ContentType);

            return Ok();
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/storage/delete/{fileName}")]
        public async Task<IActionResult> Delete(string culture, string channelId, string fileName)
        {
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            await _blobStorageServices.DeleteFileAsync(channel.Channel.Id, fileName);
            return Ok();
        }
    }
}
