using clsCms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using wppCms.Models;

namespace wppCms.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppConfigModel _appConfig;

        public HomeController(ILogger<HomeController> logger,
             IOptions<AppConfigModel> appConfig)
        {
            _logger = logger;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Show service top page.
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        [Route("/")]
        [Route("/{culture}")]
        public IActionResult Index(string culture = "en")
        {
            var view = new HomeIndexViewModel()
            {
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Show error page.
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}