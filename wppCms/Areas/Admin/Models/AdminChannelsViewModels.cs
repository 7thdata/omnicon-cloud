using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Admin.Models
{
    public class AdminChannelsViewModels
    {
    }
    public class AdminChannelsIndexViewModel : PageViewModel
    {
        public PaginationModel<ChannelModel> Channels { get; set; }
    }
    public class AdminChannelsDetailsViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
    }
    public class AdminChannelsChannelMemberViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public ChannelMembershipModel ChannelMembership { get; set; }
    }
}
