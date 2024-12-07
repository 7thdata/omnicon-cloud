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

        public ArticlesController(IArticleServices articleServices, IAuthorServices authorServices, UserManager<UserModel> userManager, IBlobStorageServices blobStorageServices)
        {
            _articleServices = articleServices;
            _authorServices = authorServices;
            _userManager = userManager;
            _blobStorageServices = blobStorageServices;
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
        [Route("/{culture}/usr/channel/{channelId}/article/upload-asset")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var contentType = file.ContentType;
            var fileName = Path.GetFileName(file.FileName);

            using var stream = file.OpenReadStream();
            var (originalUrl, thumbnailUrl) = await _blobStorageServices.UploadImageWithThumbnailAsync(stream, fileName, contentType);

            return Ok(new { originalUrl, thumbnailUrl });
        }
    }
}
