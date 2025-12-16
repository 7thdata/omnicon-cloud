
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wppCms.Areas.Admin.Models;

namespace wppCms.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class HomeController : Controller
    {
        private AppConfigModel _appConfig;

        public HomeController(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig.Value;
        }

        [Route("/{culture}/admin")]
        public IActionResult Index(string culture){

            var view = new AdminHomeIndexViewModel()
            {
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }
    }
}