using clsCms.Models;

namespace apiCms.Models
{
    public class GetArticleRequestModel
    {
        public string channelId { get; set; }
        public string articleId { get; set; }
    }

    public class GetArticleResponseModel
    {
        public ArticleViewModel Article { get; set; }
    }

    public class GetArticlesRequestModel
    {
        public string keyword { get; set; }
        public string sort { get; set; }
        public string channelId { get; set; }
        public string folderId { get; set; }
        public string authorId { get; set; }
        public string culture { get; set; }
        public int itemsPerPage { get; set; }
        public int currentPage { get; set; }
    }

    public class GetArticlesResponseModel
    {
        public PaginationModel<ArticleModel> Articles { get; set; }
    }
}
