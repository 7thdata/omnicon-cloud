using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class AnalyticsController : Controller
    {
        [Route("/{culture}/usr/analytics")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
