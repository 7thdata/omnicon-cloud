using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using clsCms.Models;
using wppCms.Areas.Usr.Models;
using System.Globalization;
using Microsoft.Extensions.Options;

namespace wppCms.Areas.usr.Controllers
{
    [Area("Usr")]
    [Authorize] 
    public class UserManagerController : Controller
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;
        private readonly AppConfigModel _appConfig;

        public UserManagerController(UserManager<UserModel> userManager, 
            SignInManager<UserModel> signInManager,
            IOptions<AppConfigModel> appConfig)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _appConfig = appConfig.Value;
        }

        // Show User Information
        [Route("/{culture}/usr/usermanagement")]
        public async Task<IActionResult> Index(string culture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var view = new UsrUserManagementIndexViewModel()
            {
                Culture = culture,
                User = user,
                GaTagId = _appConfig.Ga.TagId
            };

            // Pass user information to the view
            return View(view);
        }

        // Change Password
        [HttpGet]
        [Route("/{culture}/usr/change-password")]
        public IActionResult ChangePassword(string culture)
        {
            var view = new UsrUserManagementChangePasswordViewModel()
            {
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/usr/change-password")]
        public async Task<IActionResult> ChangePassword(UsrUserManagementChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Use specific TempData key for password change success
            TempData["PasswordChangeSuccessMessage"] = "Your password has been changed successfully.";

            // Refresh sign-in and redirect to the index page
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", new { @culture = model.Culture });
        }


        // Change Email (Username)
        [HttpGet]
        [Route("/{culture}/usr/change-email")]
        public IActionResult ChangeEmail(string culture)
        {

            var view = new UsrUserManagementChangeEmailViewModel()
            {
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/usr/change-email")]
        public async Task<IActionResult> ChangeEmail(string newEmail, string culture)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var changeEmailResult = await _userManager.ChangeEmailAsync(user, newEmail, token);
            if (!changeEmailResult.Succeeded)
            {
                foreach (var error in changeEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }

            // Update the username to the new email
            await _userManager.SetUserNameAsync(user, newEmail);

            TempData["EmailChangeSuccessMessage"] = "Your email has been changed successfully.";

            // Sign the user in again with the new credentials
            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", new { @culture = culture });
        }

        // Add these actions to the UserManagerController
        [HttpGet]
        [Route("/{culture}/usr/update-profile")]
        public async Task<IActionResult> UpdateProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var view = new UsrUserManagementUpdateProfileViewModel()
            {
                NickName = user.NickName
            };

            // Pass current NickName (Display Name) to the view
            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/usr/update-profile")]

        public async Task<IActionResult> UpdateProfile(UsrUserManagementUpdateProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update the user's display name (NickName)
            user.NickName = model.NickName;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", new { @culture = model.Culture }); // Redirect back to profile page after update
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

    }
}
