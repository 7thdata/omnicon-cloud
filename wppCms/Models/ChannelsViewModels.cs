using clsCms.Models;

namespace wppCms.Models
{
    public class ChannelsViewModels
    {
    }

    public class ChannelsIndexViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public PaginationModel<ArticleModel> Articles { get; set; }
        public string Culture { get; set; }
    }

    public class ChannelsDetailsViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public ArticleViewModel Article { get; set; }
        public string Culture { get; set; }
    }
}
