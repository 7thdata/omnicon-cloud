using AspNetCoreGeneratedDocument;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;
using wppCms.Models;

namespace wppCms.Controllers
{
    public class ChannelsController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly IChannelServices _channelServices;
        private readonly IArticleServices _articleServices;
        private readonly ISearchServices _searchServices;
        private readonly IAuthorServices _authorServices;
        private readonly IUserServices _userServices;
        private readonly AppConfigModel _appConfig;

        public ChannelsController(IChannelServices channelServices, ISearchServices searchServices,
            IArticleServices articleServices, IAuthorServices authorServices,
            IOptions<AppConfigModel> appConfig, UserManager<UserModel> userManager, IUserServices userServices)
        {
            _channelServices = channelServices;
            _articleServices = articleServices;
            _authorServices = authorServices;
            _appConfig = appConfig.Value;
            _searchServices = searchServices;
            _userManager = userManager;
            _userServices = userServices;
        }

        /// <summary>
        /// Show list of articles.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="permaName"></param>
        /// <param name="keyword"></param>
        /// <param name="author"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/c/{permaName}")]
        public async Task<IActionResult> Index(string culture, string permaName,
    string keyword, string author, string folder,string tag, string sort = "publishdate_desc", int currentPage = 1, int itemsPerPage = 100)
        {

            var user = await _userManager.GetUserAsync(User);

            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(permaName);
            if (channel == null) return NotFound();

            if (channel.Channel.IsTopPageStaticPage)
            {
                if (string.IsNullOrEmpty(channel.Channel.TopPagePermaName)) return NotFound();

                var topArticle = await _articleServices.GetArticleViewByPermaNameAsync(channel.Channel.Id,
                    channel.Channel.TopPagePermaName,culture,false);

                if(topArticle == null) return NotFound();

                var view = new ChannelsIndexViewModel
                {
                    CurrentUser = user,
                    Culture = culture,
                    Channel = channel,
                    TopArticle = topArticle,    
                    GaTagId = _appConfig.Ga.TagId,
                    FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
                };

                return View(view);
            }
            else
            {
                var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
                channel.Authors = authors;

                var searchQueryHistory = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
                channel.SearchQueryHistory = searchQueryHistory;

                // Log the search keyword if it's not empty
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    await _articleServices.RecordSearchKeywordAsync(channel.Channel.Id, keyword);
                }

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

                var articles = searchResults.GetResults().Select(result => result.Document).ToList();
                var totalCount = searchResults.TotalCount ?? 0;

                var facets = searchResults.Facets.ToDictionary(
                    facet => facet.Key,
                    facet => facet.Value.Select(f => new FacetValue { Value = f.Value.ToString(), Count = f.Count ?? 0 }));

                var view = new ChannelsIndexViewModel
                {
                    CurrentUser = user,
                    Culture = culture,
                    Channel = channel,
                    Articles = articles,
                    TotalCount = totalCount,
                    CurrentPage = currentPage,
                    ItemsPerPage = itemsPerPage,
                    Keyword = keyword,
                    Sort = sort,
                    GaTagId = _appConfig.Ga.TagId,
                    FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                    Facets = facets,
                    Tag = tag,
                    Folder = folder,
                    Author = author
                };

                return View(view);
            }
        }

        
        /// <summary>
        /// Add comment.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="articleId"></param>
        /// <param name="commentText"></param>
        /// <returns></returns>
        [HttpPost("/{culture}/c/{channelId}/a/{articleId}/comments")]
        public async Task<IActionResult> AddComment(string culture, string channelId, string articleId, string commentText)
        {
            var channel = await _channelServices.GetChannelAsync(channelId);
            var user = await _userManager.GetUserAsync(User);

            // Validate the comment text
            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["ErrorMessage"] = "Comment text cannot be empty.";
                return RedirectToAction("Details", new { culture, channelId, id = articleId });
            }

            // Fetch the article from the database
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, articleId);
            if (article == null)
            {
                TempData["ErrorMessage"] = "Article not found.";
                return RedirectToAction("Details", new { culture, channelId, id = articleId });
            }

            // Deserialize existing comments
            var comments = string.IsNullOrEmpty(article.CommentJsonString)
                ? new List<ArticleCommentModel>()
                : JsonSerializer.Deserialize<List<ArticleCommentModel>>(article.CommentJsonString);

            // Create a new comment
            var newComment = new ArticleCommentModel
            {
                CommentId = Guid.NewGuid().ToString(),
                CommentText = commentText,
                CommentById = user.Id,
                CommentByName = user.NickName,
                CommentByIconUrl = user.IconImage ?? "/images/profile/image/s512_f_object_174_0bg.png", // Replace with actual default URL or user-specific URL
                Timestamp = DateTimeOffset.UtcNow
            };

            // Add the new comment to the list
            comments.Add(newComment);

            // Update the CommentJsonString
            article.CommentJsonString = JsonSerializer.Serialize(comments);

            // Save the updated article
            await _articleServices.UpdateArticleAsync(article);

            TempData["SuccessMessage"] = "Your comment has been added.";
            return RedirectToAction("Details", new { culture, @channelName = channel.Channel.PermaName, @permaName = article.PermaName });
        }

        /// <summary>
        /// Delete comment.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="articleId"></param>
        /// <param name="commentId"></param>
        /// <returns></returns>
        [HttpPost("/{culture}/c/{channelId}/a/{articleId}/comments/{commentId}/delete")]
        public async Task<IActionResult> DeleteComment(string culture, string channelId, string articleId, string commentId)
        {
            var channel = await _channelServices.GetChannelAsync(channelId);
            // Fetch the article from the database
            var article = await _articleServices.GetArticleByChannelIdAndIdAsync(channelId, articleId);
            if (article == null)
            {
                TempData["ErrorMessage"] = "Article not found.";
                return RedirectToAction("Details", new { culture, channelId, id = articleId });
            }

            // Deserialize existing comments
            var comments = string.IsNullOrEmpty(article.CommentJsonString)
                ? new List<ArticleCommentModel>()
                : JsonSerializer.Deserialize<List<ArticleCommentModel>>(article.CommentJsonString);

            // Find the comment to delete
            var commentToDelete = comments.FirstOrDefault(c => c.CommentId == commentId);
            if (commentToDelete == null)
            {
                TempData["ErrorMessage"] = "Comment not found.";
                return RedirectToAction("Details", new { culture, channelId, id = articleId });
            }

            // Check if the current user is the author of the comment or has permission to delete
            if (commentToDelete.CommentById != User?.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            {
                TempData["ErrorMessage"] = "You do not have permission to delete this comment.";
                return RedirectToAction("Details", new { culture, channelId, id = articleId });
            }

            // Remove the comment from the list
            comments.Remove(commentToDelete);

            // Update the CommentJsonString
            article.CommentJsonString = JsonSerializer.Serialize(comments);

            // Save the updated article
            await _articleServices.UpdateArticleAsync(article);

            TempData["SuccessMessage"] = "Comment deleted successfully.";
            return RedirectToAction("Details", new { culture, @channelName = channel.Channel.PermaName, permaName = article.PermaName });
        }
        
        /// <summary>
        /// Show the article.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelName"></param>
        /// <param name="permaName"></param>
        /// <returns></returns>
        [Route("/{culture}/c/{channelName}/d/{permaName}")]
        public async Task<IActionResult> Details(string culture, string channelName, string permaName)
        {
            var user = await _userManager.GetUserAsync(User);
            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(channelName);

            // Append authors on channel
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors;

            // Append search query data
            var searchQueryHistory = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
            channel.SearchQueryHistory = searchQueryHistory;

            // Want to get search facets etc for the parition
            var articles = await _searchServices.GetFacetsAsync(
                partitionKey: channel.Channel.Id,
                isArticle: true,
                isArchived: false,
            showAuthor: true);

            var article = await _articleServices.GetArticleViewByPermaNameAsync(
                 channelId: channel.Channel.Id,
                 permaName: permaName,
                 culture: culture,
                 isPubslishDateSensitive: true);

            if (article == null || article.Article == null)
            {
                return NotFound();
            }

            var tags = "";
            if (string.IsNullOrEmpty(article.Article.Tags))
            {
                tags = channel.Channel.Title;
            }
            else
            {
                tags = article.Article.Tags;
            }

            var relatedArticlesResults = await _searchServices.SearchRelatedArticlesAsync(
                partitionKey: channel.Channel.Id,
                currentArticleId: article.Article.RowKey,
                tags: tags.Split(","),
                culture: culture,
                pageSize: 5);

            var relatedArticles = relatedArticlesResults.GetResults().Select(r => r.Document).ToList();

            // Log the impression
            await LogImpressionAsync(HttpContext, article.Article, channel, culture);

            var view = new ChannelsDetailsViewModel()
            {
                CurrentUser = user,
                Culture = culture,
                Article = article,
                Channel = channel,
                GaTagId = _appConfig.Ga.TagId,
                Facets = articles,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                RelatedArticles = relatedArticles
            };

            return View(view);
        }

        /// <summary>
        /// Log impression.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="article"></param>
        /// <param name="channel"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        private async Task LogImpressionAsync(HttpContext httpContext, ArticleModel article, ChannelViewModel channel, string culture)
        {
            // Extract relevant data from HttpContext
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            var referrer = httpContext.Request.Headers["Referer"].ToString();

            // Placeholder values for city and country
            string city = "Unknown";
            string country = "Unknown";

            // Use a geolocation service (e.g., MaxMind or API) to get city and country
            if (!string.IsNullOrEmpty(ipAddress))
            {
                try
                {
                    // Replace this with your geolocation logic
                    (city, country) = await GetGeoInfoAsync(ipAddress);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Geolocation lookup failed: {ex.Message}");
                }
            }

            // Create an impression model
            var impression = new ArticleImpressionModel
            {
                ArticleId = article.RowKey,
                AuthorId = article.AuthorId,
                Browser = userAgent,
                ChannelId = channel.Channel.Id,
                City = city,
                Country = country,
                Culture = culture,
                DeviceId = "", // Implement if needed
                FolderId = article.Folders,
                ImpressionId = Guid.NewGuid().ToString(),
                ImpressionTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                Language = culture,
                OrganizationId = channel.Channel.OrganizationId,
                Os = "", // Parse from User-Agent if needed
                Referrer = referrer,
                Tags = article.Tags,
                UserAgent = userAgent,
                UserId = httpContext.User.Identity.IsAuthenticated
                    ? httpContext.User.FindFirst("sub")?.Value
                    : ""
            };

            // Log the impression
            await _articleServices.LogArticleImpressionAsync(impression);
        }

        /// <summary>
        /// Get location.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        private async Task<(string city, string country)> GetGeoInfoAsync(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return ("Unknown", "Unknown");

            try
            {
                using var httpClient = new HttpClient();
                // IP-API endpoint for geolocation lookup
                var response = await httpClient.GetStringAsync($"http://ip-api.com/json/{ipAddress}");
                var json = Newtonsoft.Json.Linq.JObject.Parse(response);

                // Extract city and country from the JSON response
                string city = json["city"]?.ToString() ?? "Unknown";
                string country = json["country"]?.ToString() ?? "Unknown";

                return (city, country);
            }
            catch (Exception ex)
            {
                // Log or handle errors gracefully
                Console.WriteLine($"Error in geolocation lookup: {ex.Message}");
                return ("Unknown", "Unknown");
            }
        }

        /// <summary>
        /// Subscribe to the channel.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost("/{culture}/c/{channelId}/subscribe")]
        public async Task<IActionResult> SubscribeToTheChannel(string culture, string channelId, string name, string email)
        {
            try
            {
                // Fetch the channel details
                var channel = await _channelServices.GetChannelAsync(channelId);
                if (channel == null)
                {
                    TempData["ErrorMessage"] = "Channel not found.";
                    return RedirectToAction("Index", new { culture });
                }

                // Attempt to subscribe the user to the channel
                var subscription = await _channelServices.SubscribeToTheChannelAsync(channelId, email, email, "subscriber", name);
                if (subscription == null)
                {
                    TempData["ErrorMessage"] = "Subscription failed. Please try again.";
                    return RedirectToAction("Index", new { culture, @permaName = channel.Channel.PermaName });
                }

                // See if this user is registered members
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Then register the user.
                    var newUser = new UserModel
                    {
                        Email = email,
                        UserName = email,
                        NickName = name,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(newUser);

                    // Issue login link
                    var loginLink = await _userServices.IssueLoginLinkAsync(newUser.Id);

                    // Send login link to the user
                }
                else
                {

                    // Send login link to the user
                }

                // Success message
                TempData["SuccessMessage"] = "You have successfully subscribed to the channel!";
            }
            catch (Exception ex)
            {
                // Log the exception and set an error message
                TempData["ErrorMessage"] = $"An error occurred while subscribing: {ex.Message}";
            }

            return RedirectToAction("Index", new { culture, @permaName = channelId });
        }


        /// <summary>
        /// Ubsubscribe to the channel.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public async Task<IActionResult> UnsubscribeTheChannel(string culture, string channelId, string subscriberId)
        {
            var channel = await _channelServices.GetChannelAsync(channelId);

            try
            {
                var unsubscribeResult = await _channelServices.UnsbsribeToChannelAsync(channelId, subscriberId);

                if (unsubscribeResult != null)
                {
                    TempData["SuccessMessage"] = "You have successfully unsubscribed from the channel.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to unsubscribe. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while unsubscribing: {ex.Message}";
            }

            return RedirectToAction("Index", new { culture, @permaName = channel.Channel.PermaName });
        }
    }
}
