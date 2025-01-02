using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly AppConfigModel _appConfig;

        public SettingsController(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig.Value;
        }

        [Route("/{culture}/usr/settings")]
        public IActionResult Index(string culture)
        {
            var view = new UsrSettingsIndexViewModel()
            {
                Culture = culture
            };

            return View(view);
        }
    }
}
