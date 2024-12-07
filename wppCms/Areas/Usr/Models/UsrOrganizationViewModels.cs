using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Usr.Models
{
    public class UsrOrganizationCreateViewModel : PageViewModel
    {
        public OrganizationModel Organization { get; set; }
    }
    public class UsrOrganizationDetailsViewModel : PageViewModel
    {
        public OrganizationViewModel Organization { get; set; }
        public bool IsDefaultOrganization { get; set; } 
    }
    public class UsrOrganizationIndexViewModel : PageViewModel
    {
        public List<OrganizationViewModel> Organizations { get; set; }
        public string DefaultOrganizationId { get; set; }
    }
}
