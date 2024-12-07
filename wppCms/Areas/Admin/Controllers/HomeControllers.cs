
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace wppCms.Areas.Admin.Controllers
{
    [Authorize()]
    [Area("Admin")]
    public class HomeController : Controller
    {
        
        [Route("/admin")]
        public IActionResult Index(){
            return View();
        }
    }
}