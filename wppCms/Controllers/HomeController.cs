using System.Diagnostics;
using clsCms.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wppCms.Models;

namespace wppCms.Controllers
{
    /// <summary>
    /// Handles public-facing top-level pages such as
    /// the landing page, privacy policy, and error page.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppConfigModel _appConfig;

        /// <summary>
        /// Initializes the <see cref="HomeController"/>.
        /// </summary>
        /// <param name="logger">
        /// Logger instance used for diagnostic and operational logging.
        /// </param>
        /// <param name="appConfig">
        /// Strongly-typed application configuration loaded via IOptions.
        /// </param>
        public HomeController(
            ILogger<HomeController> logger,
            IOptions<AppConfigModel> appConfig)
        {
            _logger = logger;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Displays the service top (landing) page.
        ///
        /// This action supports both:
        /// - Root access ("/")
        /// - Culture-prefixed access ("/{culture}")
        ///
        /// Example:
        ///   /en-US
        ///   /ja-JP
        /// </summary>
        /// <param name="culture">
        /// Culture code used for localization (e.g. "en", "ja-JP").
        /// Defaults to "en" when not specified.
        /// </param>
        /// <returns>
        /// The landing page view populated with environment-specific settings.
        /// </returns>
        [Route("/")]
        [Route("/{culture}")]
        public IActionResult Index(string culture = "en")
        {
            var viewModel = new HomeIndexViewModel
            {
                Culture = culture,

                // Google Analytics configuration
                GaTagId = _appConfig.Ga.TagId,

                // Font Awesome kit URL (loaded globally on the page)
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View(viewModel);
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// </summary>
        /// <returns>
        /// Privacy policy view.
        /// </returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the generic error page.
        ///
        /// Response caching is explicitly disabled to ensure
        /// error details are never cached by browsers or proxies.
        /// </summary>
        /// <returns>
        /// Error view containing the current request identifier.
        /// </returns>
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
