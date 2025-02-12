
using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Channels;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class ArticlesController : UsrBaseController
    {
        private readonly IArticleServices _articleServices;
        private readonly ISearchServices _searchServices;
        private readonly IAuthorServices _authorServices;
        private readonly IBlobStorageServices _blobStorageServices;
        private readonly ILogger<ArticlesController> _logger;
        private readonly AppConfigModel _appConfig;

        public ArticlesController(IArticleServices articleServices, ISearchServices searchServices,
            IAuthorServices authorServices, UserManager<UserModel> userManager,
            IChannelServices channelServices,
            IBlobStorageServices blobStorageServices, ILogger<ArticlesController> logger,
            IOptions<AppConfigModel> appConfig) : base(userManager, channelServices)
        {
            _articleServices = articleServices;
            _authorServices = authorServices;
            _blobStorageServices = blobStorageServices;
            _logger = logger;
            _appConfig = appConfig.Value;
            _searchServices = searchServices;
        }

        [Route("/{culture}/usr/channel/{channelId}/_articles")]
        public async Task<IActionResult> _Articles(string culture, string channelId, string keyword, string sort = "publishdate_desc",
            int currentPage = 1, int itemsPerPage = 100)
        {
            // Fetch the list of articles for the channel
            var articles = await _articleServices.SearchArticlesAsync(
                channelId: channelId,
                searchQuery: keyword,
                currentPage: currentPage,
                itemsPerPage: itemsPerPage,
                sort: sort, isPublishDateSensitive: false);

            var view = new UsrArticles_ArticlesViewModel()
            {
                Articles = articles,
                ChannelId = channelId,
                Culture = culture
            };

            return PartialView(view);
        }

        [Route("/{culture}/usr/channel/{channelId}/article/create")]
        public async Task<IActionResult> Create(string culture, string channelId)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var authors = await _authorServices.ListAuthorsByChannelAsync(channelId);

            if (authors.Count == 0)
            {
                TempData["ErrorMessage"] = "You have to have at least 1 author.";
                // You have to have at least 1 author.
                return RedirectToAction("Authors", "Index", new { @culture = culture, @channelId = channelId });
            }

            var viewModel = new UsrArticlesCreateEditViewModel
            {
                Culture = culture,
                ArticleCulture = channel.Channel.DefaultCulture ?? "",
                RowKey = Guid.NewGuid().ToString(),
                PublishSince = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm"),  // Properly formatted for datetime-local
                PublishUntil = DateTimeOffset.MaxValue.ToString("yyyy-MM-ddTHH:mm"),  // Properly formatted for datetime-local
                Authors = authors, // Pass the list of authors
                IsEditMode = false,
                ShowAuthor = true,
                IsArchived = false,
                IsArticle = true,
                IsSearchable = true,
                PermaName = Guid.NewGuid().ToString(),
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Channel = channel
            };

            return View("ArticleForm", viewModel); // Use the same view
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/article/save")]
        public async Task<IActionResult> SaveArticle(UsrArticlesCreateEditViewModel model,
            string culture, string channelId)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var author = new AuthorModel();

            if (!string.IsNullOrEmpty(model.AuthorId))
            {
                author = await _authorServices.GetAuthorAsync(channel.Channel.Id, model.AuthorId);
            }

            DateTimeOffset? publishSince = null;
            DateTimeOffset? publishUntil = null;

            var version = 0;
            var articleId = ""; // Use for unindexing

            if (model.IsArticle)
            {
                if (!DateTimeOffset.TryParse(model.PublishSince, out var parsedSince))
                {
                    ModelState.AddModelError("PublishSince", "Invalid format for Publish Since.");
                    return View("ArticleForm", model);
                }

                publishSince = parsedSince;

                if (!string.IsNullOrEmpty(model.PublishUntil))
                {
                    if (!DateTimeOffset.TryParse(model.PublishUntil, out var parsedUntil))
                    {
                        ModelState.AddModelError("PublishUntil", "Invalid format for Publish Until.");
                        return View("ArticleForm", model);
                    }

                    publishUntil = parsedUntil; // Only set if parsing succeeds
                }
            }

            if (model.IsEditMode)
            {
                var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, model.RowKey);
                if (article == null) return NotFound();

                articleId = article.RowKey;
                version = article.Version + 1;

                // Update article fields
                article.Title = model.Title;
                article.Text = model.Text;
                article.Folders = model.Folders;
                article.Description = model.Description ?? "";
                article.MainImageUrl = model.MainImageUrl;
                article.PermaName = model.PermaName;
                article.Tags = model.Tags;
                article.CanonicalUrl = model.CanonicalUrl;
                article.PublishSince = publishSince;
                article.PublishUntil = publishUntil;
                article.IsArchived = model.IsArchived;
                article.IsArticle = model.IsArticle; // Include IsArticle
                article.ShowAuthor = model.ShowAuthor; // Include ShowAuthor
                article.AuthorId = model.ShowAuthor ? model.AuthorId : null; // Conditional based on ShowAuthor
                article.UpdatedAt = DateTimeOffset.UtcNow;
                article.Culture = model.ArticleCulture;
                article.Version = version;
                article.IsSearchable = model.IsSearchable;
                await _articleServices.UpdateArticleAsync(article);
            }
            else
            {
                model.RowKey = Guid.NewGuid().ToString();

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
                    IsArchived = model.IsArchived,
                    IsArticle = model.IsArticle, // Include IsArticle
                    ShowAuthor = model.ShowAuthor, // Include ShowAuthor
                    AuthorId = model.ShowAuthor ? model.AuthorId : null, // Conditional based on ShowAuthor
                    Folders = model.Folders,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    Version = version,
                    Culture = model.ArticleCulture,
                    IsSearchable = model.IsSearchable
                };

                await _articleServices.CreateArticleAsync(article);
            }

            // Index it.
            if (model.IsSearchable)
            {
                var tags = new List<string>();
                if(!string.IsNullOrEmpty(model.Tags))
                {
                    tags = model.Tags.Split(",").ToList();
                }

                await _searchServices.IndexArticlesAsync(new List<ArticleSearchModel>()
                {
                    new ArticleSearchModel()
                    {
                        ShowAuthor = model.ShowAuthor,
                        PublishSince = publishSince,
                        AuthorName = author?.PermaName,
                        CanonicalUrl = model.CanonicalUrl,
                        Culture = model.ArticleCulture,
                        Description = model.Description,
                        Folders = model.Folders,
                        IsArchived = model.IsArchived,
                        IsArticle = model.IsArticle,
                        MainImageUrl = model.MainImageUrl,
                        PartitionKey = channel.Channel.Id,
                        PermaName = model.PermaName,
                        PublishUntil = publishUntil,
                        RowKey = model.RowKey,
                        Tags = tags,
                        Text = model.Text,
                        Title = model.Title,
                        UpdatedAt = DateTimeOffset.UtcNow,
                        Version = version
                    }
                });
            }
            else
            {
                if (model.IsEditMode)
                {
                    await _searchServices.UnIndexArticlesAsync(new List<string>() {
                    articleId
                    });
                }

                /// And if it's new we do nothing since document is not indexed yet.
            }

            return RedirectToAction("Details", new { culture, channelId, id = model.RowKey });
        }

        [Route("/{culture}/usr/channel/{channelId}/article/edit/{id}")]
        public async Task<IActionResult> Edit(string culture, string channelId, string id)
        {
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, id);

            if (article == null)
            {
                return NotFound();
            }

            var authors = await _authorServices.ListAuthorsByChannelAsync(channelId);

            var viewModel = new UsrArticlesCreateEditViewModel
            {
                Channel = channel,
                Culture = culture,
                RowKey = id,
                Title = article.Title,
                Text = article.Text,
                PermaName = article.PermaName,
                Tags = article.Tags,
                CanonicalUrl = article.CanonicalUrl,
                PublishSince = article.PublishSince?.ToString("yyyy-MM-ddTHH:mm"),
                PublishUntil = article.PublishUntil?.ToString("yyyy-MM-ddTHH:mm"),
                Folders = article.Folders,
                Authors = authors, // Pass the list of authors
                AuthorId = article.AuthorId, // Set the selected author
                IsEditMode = true, // Indicate that this is an edit action,
                ArticleCulture = article.Culture,
                Description = article.Description,
                MainImageUrl = article.MainImageUrl,
                IsSearchable = article.IsSearchable,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View("ArticleForm", viewModel); // Use the same view
        }

        [Route("/{culture}/usr/channel/{channelId}/article/details/{id}")]
        public async Task<IActionResult> Details(string culture, string channelId, string id)
        {
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, id);

            if (article == null)
            {
                return NotFound();
            }

            // Create and populate the new UsrHomeArticleDetailsViewModel
            var viewModel = new UsrHomeArticleDetailsViewModel
            {
                Channel = channel,
                Article = article,
                ViewCount = 0,
                LikeCount = 0,
                CommentCount = 0,
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
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

            // Delete the article
            await _articleServices.DeleteArticleAsync(channelId, rowKey);

            // Delete from search too
            await _searchServices.UnIndexArticlesAsync(new List<string>() { rowKey });

            // Set a success message in TempData
            TempData["SuccessMessage"] = "The article has been successfully deleted.";

            // Redirect back to the channel's article list or a confirmation page
            return RedirectToAction("Channel","Home", new { culture = culture, channelId = channelId });
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/article/upload-assets")]
        public async Task<IActionResult> UploadImages(string culture, string channelId, List<IFormFile> files)
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
                    var (originalUrl, thumbnailUrl) = await _blobStorageServices.UploadImageWithThumbnailAsync(channelId, stream, fileName, file.ContentType);

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

        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/article/upload-image")]
        public async Task<IActionResult> UploadImage(string culture, string channelId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest(new { message = "No file uploaded." });
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB limit
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };

            if (file.Length > maxFileSize)
            {
                _logger.LogWarning("File {FileName} exceeds size limit.", file.FileName);
                return BadRequest(new { message = $"File size exceeds the 5MB limit." });
            }

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                _logger.LogWarning("Unsupported content type for file: {FileName}", file.FileName);
                return BadRequest(new { message = $"Unsupported file type." });
            }

            var fileName = Path.GetFileName(file.FileName);

            try
            {
                using var stream = file.OpenReadStream();

                // Optional: Validate the file's MIME type using its content
                if (!IsValidImageFile(stream))
                {
                    _logger.LogWarning("Invalid image content for file: {FileName}", fileName);
                    return BadRequest(new { message = "Invalid image file." });
                }

                // Upload file to blob storage and generate URL
                var (originalUrl, _) = await _blobStorageServices.UploadImageWithThumbnailAsync(channelId, stream, fileName, file.ContentType);

                if (string.IsNullOrWhiteSpace(originalUrl))
                {
                    _logger.LogError("Generated URL is null or empty for file: {FileName}", fileName);
                    return StatusCode(500, new { message = "Failed to upload image." });
                }

                _logger.LogInformation("File {FileName} uploaded successfully.", fileName);
                return Ok(new { location = originalUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while uploading file: {FileName}", fileName);
                return StatusCode(500, new { message = "An error occurred while uploading the image." });
            }
        }

        // Helper method to validate image file content
        private bool IsValidImageFile(Stream stream)
        {
            // Read the first 8 bytes (enough for most file signatures)
            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0, SeekOrigin.Begin); // Reset stream position to the start

            // Check for JPEG magic numbers (FF D8 FF)
            bool isJpeg = buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;

            // Check for PNG magic numbers (89 50 4E 47)
            bool isPng = buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;

            // Check for GIF magic numbers (47 49 46 38)
            bool isGif = buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38;

            // Return true if the file is JPEG, PNG, or GIF
            return isJpeg || isPng || isGif;
        }
    }
}
