using clsCms.Models;

namespace wppCms.Models
{
    public class ChannelsViewModels
    {
    }

    public class ChannelsIndexViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public PaginationModel<ArticleModel> Articles { get; set; }
    }

    public class ChannelsDetailsViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public ArticleViewModel Article { get; set; }
    }
}
