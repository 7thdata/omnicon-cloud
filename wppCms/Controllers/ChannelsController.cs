using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Json;
using wppCms.Models;

namespace wppCms.Controllers
{
    /// <summary>
    /// Handles public channel pages including:
    /// - Channel top page
    /// - Article listing and filtering
    /// - Article detail pages
    /// - Comments
    /// - Subscriptions
    ///
    /// This controller is intentionally public-facing and SEO-oriented.
    /// </summary>
    public class ChannelsController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly IChannelServices _channelServices;
        private readonly IArticleServices _articleServices;
        private readonly ISearchServices _searchServices;
        private readonly IAuthorServices _authorServices;
        private readonly IUserServices _userServices;
        private readonly AppConfigModel _appConfig;

        /// <summary>
        /// Initializes the <see cref="ChannelsController"/>.
        /// </summary>
        public ChannelsController(
            IChannelServices channelServices,
            ISearchServices searchServices,
            IArticleServices articleServices,
            IAuthorServices authorServices,
            IUserServices userServices,
            UserManager<UserModel> userManager,
            IOptions<AppConfigModel> appConfig)
        {
            _channelServices = channelServices;
            _searchServices = searchServices;
            _articleServices = articleServices;
            _authorServices = authorServices;
            _userServices = userServices;
            _userManager = userManager;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Displays a channel top page.
        ///
        /// Behavior differs based on channel configuration:
        /// - Static top page → render a single article
        /// - Dynamic channel → render article list with search and filters
        /// </summary>
        [Route("/{culture}/c/{permaName}")]
        public async Task<IActionResult> Index(
            string culture,
            string permaName,
            string keyword,
            string author,
            string folder,
            string tag,
            string sort = "publishdate_desc",
            int currentPage = 1,
            int itemsPerPage = 100)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(permaName);
            if (channel == null)
                return NotFound();

            // ----------------------------------------
            // Static top-page channel
            // ----------------------------------------
            if (channel.Channel.IsTopPageStaticPage)
            {
                if (string.IsNullOrEmpty(channel.Channel.TopPagePermaName))
                    return NotFound();

                var topArticle = await _articleServices.GetArticleViewByPermaNameAsync(
                    channel.Channel.Id,
                    channel.Channel.TopPagePermaName,
                    culture,
                    isPubslishDateSensitive: false);

                if (topArticle == null)
                    return NotFound();

                return View(new ChannelsIndexViewModel
                {
                    CurrentUser = currentUser,
                    Culture = culture,
                    Channel = channel,
                    TopArticle = topArticle,
                    GaTagId = _appConfig.Ga.TagId,
                    FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
                });
            }

            // ----------------------------------------
            // Dynamic article listing channel
            // ----------------------------------------

            // Attach authors and historical search keywords
            channel.Authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.SearchQueryHistory =
                await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);

            // Record keyword usage for analytics
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                await _articleServices.RecordSearchKeywordAsync(channel.Channel.Id, keyword);
            }

            // Execute Azure Search query
            var searchResults = await _searchServices.SearchArticlesAsync(
                partitionKey: channel.Channel.Id,
                query: keyword,
                isArticle: true,
                showAuthor: true,
                pageSize: itemsPerPage,
                pageNumber: currentPage,
                author: author,
                folder: folder,
                tag: tag,
                sort: sort,
                culture: culture);

            var articles = searchResults.GetResults()
                .Select(r => r.Document)
                .ToList();

            var facets = searchResults.Facets.ToDictionary(
                f => f.Key,
                f => f.Value.Select(v =>
                    new FacetValue
                    {
                        Value = v.Value?.ToString() ?? "",
                        Count = v.Count ?? 0
                    }));

            return View(new ChannelsIndexViewModel
            {
                CurrentUser = currentUser,
                Culture = culture,
                Channel = channel,
                Articles = articles,
                TotalCount = searchResults.TotalCount ?? 0,
                CurrentPage = currentPage,
                ItemsPerPage = itemsPerPage,
                Keyword = keyword,
                Sort = sort,
                Facets = facets,
                Tag = tag,
                Folder = folder,
                Author = author,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            });
        }

        /// <summary>
        /// Displays a single article detail page.
        /// </summary>
        [Route("/{culture}/c/{channelName}/d/{permaName}")]
        public async Task<IActionResult> Details(string culture, string channelName, string permaName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(channelName);

            if (channel == null)
                return NotFound();

            // Attach channel metadata
            channel.Authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.SearchQueryHistory =
                await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);

            // Load article
            var articleView = await _articleServices.GetArticleViewByPermaNameAsync(
                channel.Channel.Id,
                permaName,
                culture,
                isPubslishDateSensitive: true);

            if (articleView?.Article == null)
                return NotFound();

            // Load facets for sidebar filtering
            var facets = await _searchServices.GetFacetsAsync(
                partitionKey: channel.Channel.Id,
                isArticle: true,
                isArchived: false,
                showAuthor: true);

            // Resolve tags for related article search
            var tags = string.IsNullOrEmpty(articleView.Article.Tags)
                ? channel.Channel.Title
                : articleView.Article.Tags;

            var relatedArticles = (await _searchServices.SearchRelatedArticlesAsync(
                partitionKey: channel.Channel.Id,
                currentArticleId: articleView.Article.RowKey,
                tags: tags.Split(','),
                culture: culture,
                pageSize: 5))
                .GetResults()
                .Select(r => r.Document)
                .ToList();

            // Log impression (page view analytics)
            await LogImpressionAsync(HttpContext, articleView.Article, channel, culture);

            return View(new ChannelsDetailsViewModel
            {
                CurrentUser = currentUser,
                Culture = culture,
                Channel = channel,
                Article = articleView,
                Facets = facets,
                RelatedArticles = relatedArticles,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            });
        }

        /// <summary>
        /// Adds a user comment to an article.
        /// Comments are stored as JSON for flexible schema evolution.
        /// </summary>
        [HttpPost("/{culture}/c/{channelId}/a/{articleId}/comments")]
        public async Task<IActionResult> AddComment(
            string culture,
            string channelId,
            string articleId,
            string commentText)
        {
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["ErrorMessage"] = "Comment text cannot be empty.";
                return RedirectToAction("Details");
            }

            var user = await _userManager.GetUserAsync(User);
            var channel = await _channelServices.GetChannelAsync(channelId);
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, articleId);

            if (article == null || user == null)
                return NotFound();

            var comments = string.IsNullOrEmpty(article.CommentJsonString)
                ? new List<ArticleCommentModel>()
                : JsonSerializer.Deserialize<List<ArticleCommentModel>>(article.CommentJsonString)
                    ?? new List<ArticleCommentModel>();

            comments.Add(new ArticleCommentModel
            {
                CommentId = Guid.NewGuid().ToString(),
                CommentText = commentText,
                CommentById = user.Id,
                CommentByName = user.NickName,
                CommentByIconUrl = user.IconImage
                    ?? "/images/profile/image/s512_f_object_174_0bg.png",
                Timestamp = DateTimeOffset.UtcNow
            });

            article.CommentJsonString = JsonSerializer.Serialize(comments);
            await _articleServices.UpdateArticleAsync(article);

            TempData["SuccessMessage"] = "Your comment has been added.";

            return RedirectToAction(
                "Details",
                new
                {
                    culture,
                    channelName = channel.Channel.PermaName,
                    permaName = article.PermaName
                });
        }

        /// <summary>
        /// Deletes a comment authored by the current user.
        /// </summary>
        [HttpPost("/{culture}/c/{channelId}/a/{articleId}/comments/{commentId}/delete")]
        public async Task<IActionResult> DeleteComment(
            string culture,
            string channelId,
            string articleId,
            string commentId)
        {
            var channel = await _channelServices.GetChannelAsync(channelId);
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, articleId);

            if (article == null)
                return NotFound();

            var comments = string.IsNullOrEmpty(article.CommentJsonString)
                ? new List<ArticleCommentModel>()
                : JsonSerializer.Deserialize<List<ArticleCommentModel>>(article.CommentJsonString)
                    ?? new List<ArticleCommentModel>();

            var target = comments.FirstOrDefault(c => c.CommentId == commentId);
            if (target == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (target.CommentById != currentUserId)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this comment.";
                return RedirectToAction("Details");
            }

            comments.Remove(target);
            article.CommentJsonString = JsonSerializer.Serialize(comments);
            await _articleServices.UpdateArticleAsync(article);

            TempData["SuccessMessage"] = "Comment deleted successfully.";

            return RedirectToAction(
                "Details",
                new
                {
                    culture,
                    channelName = channel.Channel.PermaName,
                    permaName = article.PermaName
                });
        }

        /// <summary>
        /// Logs an article impression for analytics purposes.
        /// </summary>
        private async Task LogImpressionAsync(
            HttpContext httpContext,
            ArticleModel article,
            ChannelViewModel channel,
            string culture)
        {
            var ip = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var referrer = httpContext.Request.Headers["Referer"].ToString();

            var (city, country) = await GetGeoInfoAsync(ip);

            var impression = new ArticleImpressionModel
            {
                ImpressionId = Guid.NewGuid().ToString(),
                ArticleId = article.RowKey,
                AuthorId = article.AuthorId,
                ChannelId = channel.Channel.Id,
                OrganizationId = channel.Channel.OrganizationId,
                Culture = culture,
                Language = culture,
                ImpressionTime = DateTime.UtcNow,
                IpAddress = ip,
                Browser = userAgent,
                UserAgent = userAgent,
                Referrer = referrer,
                City = city,
                Country = country,
                Tags = article.Tags,
                UserId = httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirst("sub")?.Value
                    : ""
            };

            await _articleServices.LogArticleImpressionAsync(impression);
        }

        /// <summary>
        /// Resolves city and country information from an IP address.
        /// Uses a public IP geolocation API.
        /// </summary>
        private async Task<(string city, string country)> GetGeoInfoAsync(string? ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return ("Unknown", "Unknown");

            try
            {
                using var http = new HttpClient();
                var json = await http.GetStringAsync($"http://ip-api.com/json/{ipAddress}");
                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);

                return (
                    obj["city"]?.ToString() ?? "Unknown",
                    obj["country"]?.ToString() ?? "Unknown"
                );
            }
            catch
            {
                return ("Unknown", "Unknown");
            }
        }
    }
}
