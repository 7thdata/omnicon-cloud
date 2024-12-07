// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace wppCms.Areas.Identity.Pages.Account
{
    using clsCms.Interfaces;
    using clsCms.Helpers; // Ensure you have the correct namespace for your EmailContentHelper
    using clsCms.Services;

    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly INotificationServices _notificationServices;

        public ForgotPasswordModel(UserManager<UserModel> userManager, INotificationServices notificationServices)
        {
            _userManager = userManager;
            _notificationServices = notificationServices;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Generate password reset token
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                // Generate the email content using EmailContentHelper
                var emailContent = EmailContentHelper.GeneratePasswordResetEmail(user.UserName, HtmlEncoder.Default.Encode(callbackUrl));

                // Send the email using NotificationServices
                var emailSent = await _notificationServices.SendEmailAsync(Input.Email, "Reset Password", emailContent);

                if (emailSent)
                {
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                else
                {
                    // Optionally, handle the error case (e.g., log an error, show an error message)
                    ModelState.AddModelError(string.Empty, "There was an error sending the email. Please try again.");
                }
            }

            return Page();
        }
    }

}
