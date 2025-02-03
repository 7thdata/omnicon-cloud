using Azure.Search.Documents.Models;
using clsCms.Models;

namespace wppCms.Models
{
    public class ChannelsViewModels
    {
    }

    public class ChannelsIndexViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public List<ArticleSearchModel> Articles { get; set; }
        public Dictionary<string, IEnumerable<FacetValue>> Facets { get; set; }
        public long TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public string Keyword { get; set; }
        public string Sort { get; set; }
        public UserModel? CurrentUser { get; set; }
        public string Folder { get; set; }
        public string Author { get; set; }
        public string Tag { get; set; }
    }

  

    public class ChannelsDetailsViewModel : PageViewModel
    {
        public UserModel? CurrentUser { get; set; }
        public ChannelViewModel Channel { get; set; }
        public ArticleViewModel Article { get; set; }
        public IDictionary<string, IList<FacetResult>> Facets { get; set; } // For tags, folders, etc.
        public List<ArticleSearchModel> RelatedArticles { get; set; } // For related content

        public List<AdCreativeModel> Ads { get; set; }
    }
}
