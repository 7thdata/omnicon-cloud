using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class StorageController : Controller
    {
        private readonly AppConfigModel _appConfig;

        public StorageController(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig.Value;
        }

        [Route("/{culture}/usr/storage")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
