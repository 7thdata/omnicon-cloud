using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class AdsController : Controller
    {
        [Route("/{culture}/usr/adcampaigns")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
