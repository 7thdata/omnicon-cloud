using clsCms.Models;
using Microsoft.AspNetCore.Identity;


namespace wppCms.Areas.Admin.Models
{
    public class AdminUsersIndexViewModel
    {
        public PaginationModel<UserViewModel> PaginatedUsers { get; set; }
        public string Keyword { get; set; }
        public string Sort { get; set; }
    }

    public class AdminUsersDetailsViewModel
    {
        public UserModel User { get; set; } // Basic user information
        public List<IdentityRole> Roles { get; set; } // List of roles already assigned to the user
        public List<IdentityRole> AvailableRoles { get; set; } // List of available roles for assigning
    }

     public class AdminUsersRolesViewModel
    {
        public IEnumerable<IdentityRole> Roles { get; set; } // List of roles
        public string NewRoleName { get; set; } // Used for adding a new role
    }
}