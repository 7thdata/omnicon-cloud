using System.ComponentModel.DataAnnotations;
using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Usr.Models
{
    public class UsrUserManagementUpdateProfileViewModel : PageViewModel
    {
        [Required(ErrorMessage = "Display Name is required")]
        [Display(Name = "Display Name")]
        public string NickName { get; set; }
    }

    public class UsrUserManagementIndexViewModel : PageViewModel
    {
        public UserModel User { get; set; }
    }

    public class UsrUserManagementChangeEmailViewModel : PageViewModel
    {

    }

    public class UsrUserManagementChangePasswordViewModel : PageViewModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }
    }
}