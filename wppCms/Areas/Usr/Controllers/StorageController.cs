using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class StorageController : Controller
    {
        [Route("/{culture}/usr/storage")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
