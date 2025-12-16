using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration;
using wppCms.Areas.FileSharings.Models;

namespace wppCms.Areas.FileSharings.Controllers
{


    [Area("FileSharings")]
    public class HomeController : Controller
    {
        private readonly IFileSharingServices _fileSharingServices;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;

        public HomeController(IFileSharingServices fileSharingServices, UserManager<UserModel> userManager,
            SignInManager<UserModel> signInManager)
        {
            _fileSharingServices = fileSharingServices;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Route("/{culture}/filesharings")]
        public IActionResult Index(string culture)
        {
            var view = new FileSharingsHomeIndexViewModel()
            {
                Culture = culture
            };

            return View(view);
        }

        [Route("/{culture}/filesharings/files")]
        public async Task<IActionResult> Files(string culture, string boxId, string search, string sort, string name, string email,
            int currentPage = 1, int itemsPerPage = 100)
        {

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Create new user
                user = new UserModel
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true // optional, or set false and send verification if needed
                };

                var createResult = await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    return BadRequest("Failed to create user.");
                }

            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Get files
            // var files = await _fileSharingServices.GetFilesByBoxIdAsync(boxId, search, sort, currentPage, itemsPerPage);

            var view = new FileSharingsHomeFilesViewModel()
            {
                Culture = culture,
                BoxId = boxId,
                CurrentUser = user
            };

            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/filesharings/upload")]
        public async Task<IActionResult> UploadFile(string culture, string boxId, string name, string email, IFormFile uploadFile)
        {
            if (uploadFile == null || uploadFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file.";
                return RedirectToAction("Files", new { culture, boxId, name, email });
            }

            var fileModel = new FilesSharingFileModel
            {
                Id = Guid.NewGuid().ToString("N"),
                ChannelId = boxId,
                FileOwner = name,
                Email = email,
                UploadedAt = DateTime.UtcNow
            };

            // await _fileSharingServices.UpsertFileAsync(fileModel, uploadFile);

            TempData["SuccessMessage"] = "File uploaded successfully!";
            return RedirectToAction("Files", new { culture, boxId, name, email });
        }

    }
}
