using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class AnalyticsController : Controller
    {

        private readonly AppConfigModel _appConfig;

        public AnalyticsController(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig.Value;
        }

        [Route("/{culture}/usr/analytics")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
