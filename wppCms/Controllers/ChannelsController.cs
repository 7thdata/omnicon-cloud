using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using wppCms.Models;

namespace wppCms.Controllers
{
    public class ChannelsController : Controller
    {

        private readonly IChannelServices _channelServices;
        private readonly IArticleServices _articleServices;
        private readonly IAuthorServices _authorServices;
        private readonly AppConfigModel _appConfig;

        public ChannelsController(IChannelServices channelServices,
            IArticleServices articleServices, IAuthorServices authorServices,
            IOptions<AppConfigModel> appConfig)
        {
            _channelServices = channelServices;
            _articleServices = articleServices;
            _authorServices = authorServices;
            _appConfig = appConfig.Value;
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
            string keyword, string author, string sort = "publishdate_desc", int currentPage = 1, int itemsPerPage = 100)
        {
            // This is the top page of public channels.
            // User can choose to make their channle public, then can show contents here.

            // Get public channel by perma name.
            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(permaName);

            if (channel == null)
            {
                return NotFound();
            }

            // Append authors on channel
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors;

            // Append search query data
            var searchQueryHistory = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
            channel.SearchQueryHistory = searchQueryHistory;

            // Get list of articles.
            var articles = await _articleServices.SearchArticlesAsync(
                channelId: channel.Channel.Id,
                searchQuery: keyword,
                currentPage: currentPage,
                itemsPerPage: itemsPerPage,
                sort: sort,
                authorPermaName: author, isPublishDateSensitive: true);

           
            var view = new ChannelsIndexViewModel()
            {
                Culture = culture,
                Channel = channel,
                Articles = articles,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
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
            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(channelName);
            // Append authors on channel
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors;

            // Append search query data
            var searchQueryHistory = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
            channel.SearchQueryHistory = searchQueryHistory;

            var article = await _articleServices.GetArticleViewByPermaNameAsync(
                 channelId: channel.Channel.Id, 
                 permaName: permaName,
                 isPubslishDateSensitive: true);

            // Log the impression
            await LogImpressionAsync(HttpContext, article.Article, channel, culture);


            var view = new ChannelsDetailsViewModel()
            {
                Culture = culture,
                Article = article,
                Channel = channel,
                GaTagId = _appConfig.Ga.TagId
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
    }
}
