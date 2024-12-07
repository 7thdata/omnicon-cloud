using apiCms.Models;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apiCms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleServices _articleServices;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(IArticleServices articleServices, ILogger<ArticlesController> logger)
        {
            _articleServices = articleServices;
            _logger = logger;
        }

        /// <summary>
        /// Handles requests to fetch an article by its ID or PermaName.
        /// </summary>
        /// <param name="articleRequest">The request model containing the channel ID, article ID, or PermaName.</param>
        /// <returns>
        /// An ActionResult containing the article details in a GetArticleResponseModel.
        /// Returns:
        /// - 200 OK: When the article is successfully retrieved.
        /// - 400 Bad Request: When required parameters are missing.
        /// - 404 Not Found: When the article does not exist.
        /// </returns>
        [HttpPost("/article")]
        public async Task<ActionResult> GetArticleAsync(GetArticleRequestModel articleRequest)
        {
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
                article = await _articleServices.GetArticleViewAsync(articleRequest.ChannelId, articleRequest.ArticleId);
            }
            else
            {
                article = await _articleServices.GetArticleViewByPermaNameAsync(articleRequest.ChannelId, articleRequest.ArticlePermaName);
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

        [HttpPost("/articles")]
        public async Task<ActionResult> GetArticlesAsync(GetArticlesRequestModel articlesRequest)
        {
            var articles = await _articleServices.SearchArticlesAsync(articlesRequest.channelId,
                articlesRequest.keyword, articlesRequest.currentPage, articlesRequest.itemsPerPage, articlesRequest.sort);

            var result = new GetArticlesResponseModel()
            {
                Articles = articles
            };

            return Ok(result);
        }

    }
}
