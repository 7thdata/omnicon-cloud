using apiCms.Models;
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

        public ArticlesController(IArticleServices articleServices)
        {
            _articleServices = articleServices;
        }

        [HttpPost("/article")]
        public async Task<ActionResult> GetArticleAsync(GetArticleRequestModel articleRequest)
        {
            var article  = await _articleServices.GetArticleViewAsync(articleRequest.channelId, articleRequest.articleId);

            var result = new GetArticleResponseModel
            {
                Article = article
            };

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
