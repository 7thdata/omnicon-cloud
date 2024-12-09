using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly IArticleServices _articleServices;
        private readonly IAuthorServices _authorServices;
        private readonly UserManager<UserModel> _userManager;
        private readonly IBlobStorageServices _blobStorageServices;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(IArticleServices articleServices, IAuthorServices authorServices, UserManager<UserModel> userManager, IBlobStorageServices blobStorageServices, ILogger<ArticlesController> logger)
        {
            _articleServices = articleServices;
            _authorServices = authorServices;
            _userManager = userManager;
            _blobStorageServices = blobStorageServices;
            _logger = logger;
        }

        [Route("/{culture}/usr/channel/{channelId}/article/create")]
        public async Task<IActionResult> Create(string culture, string channelId)
        {
            var user = await _userManager.GetUserAsync(User);

            var authors = await _authorServices.ListAuthorsByUserIdAndOrganizationIdAsync(user.OrganizationId,_userManager.GetUserId(User), false);

            var viewModel = new UsrArticlesCreateEditViewModel
            {
                ChannelId = channelId,
                Culture = culture,
                ArticleCulture = culture,
                RowKey = Guid.NewGuid().ToString(),
                PublishSince = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm"),  // Properly formatted for datetime-local
                PublishUntil = DateTimeOffset.MaxValue.ToString("yyyy-MM-ddTHH:mm"),  // Properly formatted for datetime-local
                Authors = authors, // Pass the list of authors
                IsEditMode = false,

            };

            return View("ArticleForm", viewModel); // Use the same view
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/article/save")]
        public async Task<IActionResult> SaveArticle(UsrArticlesCreateEditViewModel model, string culture, string channelId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                // Loop through each error in ModelState to add custom messages for each
                foreach (var key in ModelState.Keys.ToList())
                {
                    foreach (var error in ModelState[key].Errors.ToList())
                    {
                        ModelState.AddModelError(key, error.ErrorMessage);
                    }
                }

                var authors = await _authorServices.ListAuthorsByUserIdAndOrganizationIdAsync(user.OrganizationId,_userManager.GetUserId(User), false);
                model.Authors = authors;

                return View("ArticleForm", model); // Return view with validation errors
            }

            // Ensure that PublishSince and PublishUntil are parsed to DateTimeOffset
            if (!DateTimeOffset.TryParse(model.PublishSince, out DateTimeOffset publishSince))
            {
                ModelState.AddModelError("PublishSince", "Invalid date and time format for Publish Since.");
                return View("ArticleForm", model);
            }

            DateTimeOffset? publishUntil = null;
            if (!string.IsNullOrEmpty(model.PublishUntil) && !DateTimeOffset.TryParse(model.PublishUntil, out DateTimeOffset parsedPublishUntil))
            {
                ModelState.AddModelError("PublishUntil", "Invalid date and time format for Publish Until.");
                return View("ArticleForm", model);
            }
            else if (!string.IsNullOrEmpty(model.PublishUntil))
            {
                publishUntil = DateTimeOffset.Parse(model.PublishUntil);
            }

            if (model.IsEditMode)
            {
                // Edit logic
                var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, model.RowKey);
                if (article == null)
                {
                    return NotFound();
                }

                // Update article fields
                article.Title = model.Title;
                article.Text = model.Text;
                article.Description = model.Description ?? "";
                article.MainImageUrl = model.MainImageUrl;
                article.PermaName = model.PermaName;
                article.Tags = model.Tags;
                article.CanonicalUrl = model.CanonicalUrl;
                article.PublishSince = publishSince;
                article.PublishUntil = publishUntil;
                article.Folders = model.Folders;
                article.AuthorId = model.AuthorId;  

                article.Culture = model.ArticleCulture;

                await _articleServices.UpdateArticleAsync(article);
            }
            else
            {
                model.RowKey = Guid.NewGuid().ToString(); // Create a new RowKey

                // Create logic
                var article = new ArticleModel
                {
                    PartitionKey = channelId,
                    RowKey = model.RowKey,
                    ChannelId = channelId,
                    Title = model.Title,
                    Text = model.Text,
                    Description = model.Description,
                    MainImageUrl = model.MainImageUrl,
                    PermaName = model.PermaName,
                    Tags = model.Tags,
                    CanonicalUrl = model.CanonicalUrl,
                    PublishSince = publishSince,
                    PublishUntil = publishUntil,
                    AuthorId = model.AuthorId, // Assume the current user is the author
                    Folders = model.Folders,
                    Culture = model.ArticleCulture
                };

                await _articleServices.CreateArticleAsync(article);
            }

            return RedirectToAction("Details", new { @culture = culture, @channelId = channelId, @id = model.RowKey });
        }

        [Route("/{culture}/usr/channel/{channelId}/article/edit/{id}")]
        public async Task<IActionResult> Edit(string culture, string channelId, string id)
        {
            var user = await _userManager.GetUserAsync(User);
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, id);

            if (article == null)
            {
                return NotFound();
            }

            var authors = await _authorServices.ListAuthorsByUserIdAndOrganizationIdAsync(user.OrganizationId, _userManager.GetUserId(User), false);

            var viewModel = new UsrArticlesCreateEditViewModel
            {
                ChannelId = channelId,
                Culture = culture,
                RowKey = id,
                Title = article.Title,
                Text = article.Text,
                PermaName = article.PermaName,
                Tags = article.Tags,
                CanonicalUrl = article.CanonicalUrl,
                PublishSince = article.PublishSince.ToString("yyyy-MM-ddTHH:mm"),
                PublishUntil = article.PublishUntil?.ToString("yyyy-MM-ddTHH:mm"),
                Folders = article.Folders,
                Authors = authors, // Pass the list of authors
                AuthorId = article.AuthorId, // Set the selected author
                IsEditMode = true, // Indicate that this is an edit action,
                ArticleCulture = article.Culture,
                Description = article.Description,
                MainImageUrl = article.MainImageUrl
            };

            return View("ArticleForm", viewModel); // Use the same view
        }


        [Route("/{culture}/usr/channel/{channelId}/article/details/{id}")]
        public async Task<IActionResult> Details(string culture, string channelId, string id)
        {
            var userId = _userManager.GetUserId(User);
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, id);

            if (article == null)
            {
                return NotFound();
            }

            // Create and populate the new UsrHomeArticleDetailsViewModel
            var viewModel = new UsrHomeArticleDetailsViewModel
            {
                Article = article,
                ViewCount = 0,
                LikeCount = 0,
                CommentCount = 0,
                Culture = culture
            };

            return View("Details", viewModel); // Use the new Details.cshtml view
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/article/delete/{rowKey}")]
        public async Task<IActionResult> DeleteArticle(string culture, string channelId, string rowKey)
        {
            var userId = _userManager.GetUserId(User);

            // Fetch the article to check if it exists
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, rowKey);

            if (article == null)
            {
                return NotFound();
            }

            // Check if the user deleting the article is the author or has permission
            if (article.AuthorId != userId)
            {
                return Forbid(); // Return a 403 Forbidden if the user is not authorized
            }

            // Delete the article
            await _articleServices.DeleteArticleAsync(channelId, rowKey);

            // Redirect back to the channel's article list or a confirmation page
            return RedirectToAction("Channel", new { culture = culture, channelId = channelId });
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/article/upload-assets")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files)
        {
            // Validate the files collection
            if (files == null || files.Count == 0)
            {
                _logger.LogWarning("No files were uploaded.");
                return BadRequest(new { message = "No files uploaded." });
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB limit
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            var uploadResults = new List<object>();

            foreach (var file in files)
            {
                // Skip invalid or empty files
                if (file.Length == 0)
                {
                    _logger.LogWarning("Skipping empty file: {FileName}", file.FileName);
                    continue;
                }

                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning("File {FileName} exceeds size limit.", file.FileName);
                    return BadRequest(new { message = $"File size for {file.FileName} exceeds the 5MB limit." });
                }

                if (!allowedContentTypes.Contains(file.ContentType))
                {
                    _logger.LogWarning("Unsupported content type for file: {FileName}", file.FileName);
                    return BadRequest(new { message = $"Unsupported file type for {file.FileName}." });
                }

                var fileName = Path.GetFileName(file.FileName);

                try
                {
                    using var stream = file.OpenReadStream();

                    // Optional: Validate the file's MIME type using its content
                    if (!IsValidImageFile(stream))
                    {
                        _logger.LogWarning("Invalid image content for file: {FileName}", fileName);
                        return BadRequest(new { message = $"Invalid image file: {fileName}" });
                    }

                    // Upload file to blob storage and generate URLs
                    var (originalUrl, thumbnailUrl) = await _blobStorageServices.UploadImageWithThumbnailAsync(stream, fileName, file.ContentType);

                    if (string.IsNullOrWhiteSpace(originalUrl) || string.IsNullOrWhiteSpace(thumbnailUrl))
                    {
                        _logger.LogError("Generated URLs are null or empty for file: {FileName}", fileName);
                        return StatusCode(500, new { message = $"Failed to generate URLs for {fileName}" });
                    }

                    // Add the result to the upload results list
                    uploadResults.Add(new { fileName, originalUrl, thumbnailUrl });
                    _logger.LogInformation("File {FileName} uploaded successfully.", fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while uploading file: {FileName}", fileName);
                    return StatusCode(500, new { message = $"An error occurred while uploading {fileName}." });
                }
            }

            if (uploadResults.Count == 0)
            {
                _logger.LogWarning("No valid files were processed.");
                return BadRequest(new { message = "No valid files were uploaded." });
            }

            return Ok(uploadResults);
        }

        // Helper method to validate image file content
        private bool IsValidImageFile(Stream stream)
        {
            // Example: Check JPEG magic numbers (you can expand this for more file types)
            byte[] buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Seek(0, SeekOrigin.Begin); // Reset stream position
            return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
        }
    }
}
