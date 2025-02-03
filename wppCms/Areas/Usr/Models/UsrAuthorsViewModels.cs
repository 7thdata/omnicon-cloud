using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Usr.Models
{
    public class UsrAuthorsIndexViewModel : PageViewModel
    {
        public List<AuthorModel> Authors { get; set; }
        public ChannelViewModel Channel { get; set; }
    }

    public class UsrAuthorsDetailsViewModel : PageViewModel
    {
        public AuthorModel Author { get; set; }
        public ChannelViewModel Channel { get; set; }
    }
}
