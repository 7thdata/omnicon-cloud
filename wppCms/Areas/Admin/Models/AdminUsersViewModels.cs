using clsCms.Models;
using Microsoft.AspNetCore.Identity;
using wppCms.Models;


namespace wppCms.Areas.Admin.Models
{
    public class AdminUsersIndexViewModel : PageViewModel
    {
        public PaginationModel<UserViewModel> PaginatedUsers { get; set; }
        public bool ShowOnlyEmailConfirmed { get; set; }
    }

    public class AdminUsersDetailsViewModel : PageViewModel
    {
        public UserModel User { get; set; } // Basic user information
        public List<IdentityRole> Roles { get; set; } // List of roles already assigned to the user
        public List<IdentityRole> AvailableRoles { get; set; } // List of available roles for assigning
    }

     public class AdminUsersRolesViewModel : PageViewModel
    {
        public IEnumerable<IdentityRole> Roles { get; set; } // List of roles
        public string NewRoleName { get; set; } // Used for adding a new role
    }

    public class AdminUsersOrganizationsViewModel : PageViewModel
    {
        public PaginationModel<OrganizationModel> Organizations { get; set; } // List of roles
    }
    public class AdminUsersOrganizationViewModel : PageViewModel
    {
        public OrganizationViewModel Organization { get; set; } // List of organizations
    }
    public class AdminUsersOrganizationMemberViewModel : PageViewModel
    {
        public OrganizationViewModel Organization { get; set; } // List of organizations
        public OrganizationMemberModel OrganizationMember { get; set; } // List of organizations
    }
}