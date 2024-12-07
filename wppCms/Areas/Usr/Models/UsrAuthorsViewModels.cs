using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Usr.Models
{
    public class UsrAuthorsIndexViewModel : PageViewModel
    {
        public List<AuthorModel> Authors { get; set; }
        public IList<ChannelViewModel> Channels { get; set; }
    }
}
