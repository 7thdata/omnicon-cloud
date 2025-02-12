using apiCms.Models;
using Azure.Search.Documents.Models;
using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace apiCms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleServices _articleServices;
        private readonly ISearchServices _searchServices;
        private readonly ILogger<ArticlesController> _logger;
        private readonly IAuthorServices _authorServices;
        private readonly UserManager<UserModel> _userManager;
        private readonly IChannelServices _channelServices;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="articleServices"></param>
        /// <param name="searchServices"></param>
        /// <param name="logger"></param>
        public ArticlesController(IArticleServices articleServices,
            ISearchServices searchServices,
            ILogger<ArticlesController> logger,
            IAuthorServices authorServices,
            UserManager<UserModel> userManager,
            IChannelServices channelServices)
        {
            _articleServices = articleServices;
            _logger = logger;
            _searchServices = searchServices;
            _authorServices = authorServices;
            _userManager = userManager;
            _channelServices = channelServices;
        }

        /// <summary>
        /// Handles requests to fetch an article by its ID or PermaName.
        /// </summary>
        /// <param name="articleRequest">
        /// The request model containing the channel ID, and either the article ID or the PermaName.
        /// </param>
        /// <returns>
        /// An ActionResult containing the article details in a GetArticleResponseModel.
        /// </returns>
        /// <response code="200">Returns the article details successfully.</response>
        /// <response code="400">
        /// Returns a BadRequest if required parameters (ChannelId, ArticleId, or ArticlePermaName) are missing.
        /// </response>
        /// <response code="404">Returns NotFound if the article does not exist.</response>
        /// <remarks>
        /// At least one of ArticleId or ArticlePermaName must be provided. 
        /// If both are provided, ArticleId takes precedence.
        /// </remarks>
        [HttpPost("/article")]
        [ProducesResponseType(typeof(GetArticleResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetArticleResponseModel>> GetArticleAsync(GetArticleRequestModel articleRequest)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var channel = await _channelServices.GetChannelAsync(articleRequest.ChannelId, user.Id);
            if (channel == null) return NotFound();


            // Validate that ChannelId is provided.
            if (string.IsNullOrEmpty(articleRequest.ChannelId))
            {
                return BadRequest(new { error = "ChannelId is required." });
            }

            // Validate that at least one of ArticleId or ArticlePermaName is provided.
            if (string.IsNullOrEmpty(articleRequest.ArticleId) && string.IsNullOrEmpty(articleRequest.ArticlePermaName))
            {
                return BadRequest(new { error = "Either ArticleId or ArticlePermaName must be provided." });
            }

            // Log the incoming request for debugging purposes.
            _logger.LogInformation("Fetching article with ChannelId: {ChannelId}, ArticleId: {ArticleId}, PermaName: {PermaName}",
                articleRequest.ChannelId, articleRequest.ArticleId, articleRequest.ArticlePermaName);

            // Initialize the article result.
            ArticleViewModel article;

            // Fetch the article by ID or PermaName.
            if (!string.IsNullOrEmpty(articleRequest.ArticleId))
            {
                article = await _articleServices.GetArticleViewAsync(articleRequest.ChannelId, articleRequest.ArticleId, articleRequest.Culture);
            }
            else
            {
                if (string.IsNullOrEmpty(articleRequest.ArticlePermaName))
                {
                    return BadRequest(new { error = "Permaname is required." });
                }

                bool isPublishDateSensitive = articleRequest.IsPublishDateSensitive ?? false;

                article = await _articleServices.GetArticleViewByPermaNameAsync(articleRequest.ChannelId,
                    articleRequest.ArticlePermaName, articleRequest.Culture, isPublishDateSensitive);
            }

            // If the article is not found, return a 404 response.
            if (article == null)
            {
                return NotFound(new { error = "Article not found." });
            }

            // Wrap the article details in a response model.
            var result = new GetArticleResponseModel
            {
                Article = article
            };

            // Return a 200 OK response with the result.
            return Ok(result);
        }

        /// <summary>
        /// Fetches a list of articles based on the provided filters, sorting, and pagination options.
        /// </summary>
        /// <param name="articlesRequest">
        /// The request model containing filtering, sorting, and pagination details for the article search.
        /// </param>
        /// <returns>
        /// A response model containing a list of articles matching the specified criteria, 
        /// along with metadata such as facets, total count, and pagination details.
        /// </returns>
        /// <response code="200">The list of articles matching the filters is returned successfully.</response>
        /// <response code="400">The request model contains invalid data, such as missing or invalid fields.</response>
        /// <response code="401">The user is not authenticated and cannot access the resource.</response>
        /// <response code="404">The specified channel was not found.</response>
        [HttpPost("/articles")]
        [ProducesResponseType(typeof(GetArticlesResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetArticlesResponseModel>> GetArticlesAsync(GetArticlesRequestModel articlesRequest)
        {
            // Retrieve the currently authenticated user
            var user = await _userManager.GetUserAsync(User);

            // If no user is authenticated, return an unauthorized response
            if (user == null)
            {
                return Unauthorized();
            }

            // Retrieve the channel information using the provided ChannelId and user ID
            var channel = await _channelServices.GetChannelAsync(articlesRequest.ChannelId, user.Id);

            // If the channel is not found, return a 404 Not Found response
            if (channel == null)
            {
                return NotFound();
            }

            // Fetch the list of authors associated with the channel
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors; // Attach the authors to the channel object

            // Fetch the search query history for the channel
            var searchQueryHistory = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
            channel.SearchQueryHistory = searchQueryHistory; // Attach the search history to the channel object

            // If the keyword in the request is not empty, log it as a search keyword for analytics
            if (!string.IsNullOrWhiteSpace(articlesRequest.Keyword))
            {
                await _articleServices.RecordSearchKeywordAsync(channel.Channel.Id, articlesRequest.Keyword);
            }

            // Perform a search for articles based on the provided parameters
            var searchResults = await _searchServices.SearchArticlesAsync(
                partitionKey: articlesRequest.ChannelId,
                query: articlesRequest.Keyword,
                isArchived: false, // Exclude archived articles
                isArticle: true, // Search only for articles
                showAuthor: true, // Include author information
                folder: articlesRequest.FolderName,
                tag: articlesRequest.Tag,
                author: articlesRequest.AuthorPermaName,
                pageSize: articlesRequest.ItemsPerPage,
                culture: articlesRequest.Culture,
                sort: articlesRequest.Sort,
                pageNumber: articlesRequest.CurrentPage);

            if (searchResults == null)
            {
                return new GetArticlesResponseModel()
                {
                    Articles = null, // The list of articles
                    Sort = articlesRequest.Sort, // Sorting preference
                    Author = articlesRequest.AuthorPermaName, // Filtered author (if any)
                    Channel = channel, // Channel details including authors and search history
                    CurrentPage = articlesRequest.CurrentPage, // Current page number for pagination
                    ItemsPerPage = articlesRequest.ItemsPerPage, // Number of items per page
                    Keyword = articlesRequest.Keyword, // Search keyword
                    Tag = articlesRequest.Tag, // Filtered tag (if any)
                    TotalCount = 0, 
                    TotalPages = 0, 
                    CurrentUser = new ProfileViewModel(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.NickName,
                    user.IconImage,
                    user.CreatedOn),
                    Facets = null,
                    Folder = null
                };
            }

            // Extract the search results and convert them to a list of article documents
            var articles = searchResults.GetResults().Select(result => result.Document).ToList();

            // Process the facets (e.g., tags, categories) returned by the search service
            var facets = searchResults.Facets.ToDictionary(
                facet => facet.Key,
                facet => facet.Value.Select(f => new FacetValue
                {
                    Value = f.Value.ToString(),
                    Count = f.Count ?? 0
                }));

            // Calculate the total number of pages for pagination
            int totalPages = 0;
            if (searchResults.TotalCount != null && searchResults.TotalCount > 0)
            {
                totalPages = ((int)searchResults.TotalCount / articlesRequest.ItemsPerPage) + 1;
            }

            // Prepare the response model with all the required data
            var result = new GetArticlesResponseModel()
            {
                Articles = articles, // The list of articles
                Sort = articlesRequest.Sort, // Sorting preference
                Author = articlesRequest.AuthorPermaName, // Filtered author (if any)
                Channel = channel, // Channel details including authors and search history
                CurrentPage = articlesRequest.CurrentPage, // Current page number for pagination
                ItemsPerPage = articlesRequest.ItemsPerPage, // Number of items per page
                Keyword = articlesRequest.Keyword, // Search keyword
                Tag = articlesRequest.Tag, // Filtered tag (if any)
                TotalCount = searchResults.TotalCount ?? 0, // Total number of articles found
                TotalPages = totalPages, // Total pages calculated for pagination
                CurrentUser = new ProfileViewModel(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.NickName,
                    user.IconImage,
                    user.CreatedOn), // Current user profile data
                Folder = articlesRequest.FolderName, // Folder filter (if any)
                Facets = facets // Facets for additional filtering options
            };

            // Return the result wrapped in an HTTP 200 OK response
            return Ok(result);
        }

        /// <summary>
        /// Get stats of articles.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("/articles-stats")]
        [ProducesResponseType(typeof(GetArticlesStatsResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetArticlesStatsResponseModel>> GetArticlesStatsAsync(GetArticleStatsRequestModel request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var channel = await _channelServices.GetChannelAsync(request.ChannelId, user.Id);
            if (channel == null) return NotFound();

            var searchQueryHistories = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
            channel.SearchQueryHistory = searchQueryHistories;

            var searchResults = await _searchServices.SearchArticlesAsync(
                partitionKey: channel.Channel.Id,
                query: "",
                isArchived: false,
                isArticle: true,
                showAuthor: true,
                folder: "",
                tag: "",
                author: "",
                pageSize: 0,
                culture: request.Culture,
                pageNumber: 0);

            if(searchResults == null)
            {
                return new GetArticlesStatsResponseModel()
                {
                    Channel = channel,
                    Facets = null
                };
            }

            var articles = searchResults.GetResults().Select(result => result.Document).ToList();

            var facets = searchResults.Facets.ToDictionary(
              facet => facet.Key,
              facet => facet.Value.Select(f => new FacetValue { Value = f.Value.ToString(), Count = f.Count ?? 0 }));

            // Attach Authors
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors;

            // Fetch the search query history for the channel
            var searchQueryHistory = await _articleServices.GetSearchKeywordHistoryAsync(channel.Channel.Id);
            channel.SearchQueryHistory = searchQueryHistory; // Attach the search history to the channel object


            var result = new GetArticlesStatsResponseModel()
            {
                Facets = facets,
                Channel = channel
            };

            return Ok(result);
        }
    }
}
