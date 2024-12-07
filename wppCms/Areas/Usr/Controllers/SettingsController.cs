using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class SettingsController : Controller
    {
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
