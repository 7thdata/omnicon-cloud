using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class OrganizationController : Controller
    {
        private readonly IUserServices _userServices;
        private readonly UserManager<UserModel> _userManager;
        private readonly AppConfigModel _appConfig;

        public OrganizationController(IUserServices userServices,
            UserManager<UserModel> userManager, IOptions<AppConfigModel> appConfig)
        {
            _userServices = userServices;
            _userManager = userManager;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Show list of organizations
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        [Route("/{culture}/usr/organizations")]
        public async Task<IActionResult> Index(string culture, string keyword, string sort)
        {
            var user = await _userManager.GetUserAsync(User);

            var myOrganizations = await _userServices.GetMyOrganizationsAsync(user.Id,keyword,sort);

            var view = new UsrOrganizationIndexViewModel()
            {
                Organizations = myOrganizations,
                Culture = culture,
                DefaultOrganizationId = user.OrganizationId,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        /// <summary>
        /// Show details of the organization
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        [Route("/{culture}/usr/organization/{organizationId}")]
        public async Task<IActionResult> Details(string culture, string organizationId)
        {
            var user = await _userManager.GetUserAsync(User);

            var organization = await _userServices.GetOrganizationViewByIdAsync(organizationId);

            var isDefaultOrganization = false;
            if(user.OrganizationId == organizationId)
            {
                isDefaultOrganization = true;
            }

            var view = new UsrOrganizationDetailsViewModel()
            {
                Organization = organization,
                Culture = culture,
                IsDefaultOrganization = isDefaultOrganization,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        [Route("/{culture}/usr/organization/create")]
        public IActionResult Create(string culture)
        {
            var view = new UsrOrganizationCreateViewModel()
            {
                Organization = new OrganizationModel()
                {
                    OrganizationId = Guid.NewGuid().ToString(),

                },
                Culture = culture
            };
            return View(view);
        }

        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/{culture}/usr/organization/upsert")]
        public async Task<IActionResult> UpsertOrganization(string culture, string organizationId, string organizationName,
            string organizationType, string organizationDescription, string organizationLogo,
            string organizationWebsite, string organizationEmail, string organizationPhone,
            string organizationAddress, string organizationCity, string organizationState,
            string organizationCountry)
        {
            var user = await _userManager.GetUserAsync(User);

            var organization = new OrganizationModel()
            {
                OrganizationId = organizationId,
                OrganizationName = organizationName,
                OrganizationType = organizationType,
                OrganizationDescription = organizationDescription,
                OrganizationLogo = organizationLogo,
                OrganizationWebsite = organizationWebsite,
                OrganizationEmail = organizationEmail,
                OrganizationPhone = organizationPhone,
                OrganizationAddress = organizationAddress,
                OrganizationCity = organizationCity,
                OrganizationState = organizationState,
                OrganizationCountry = organizationCountry,
                CreatedBy = user.Id
            };

            var upsertResult = await _userServices.UpsertOrganizationAsync(organization);

            TempData["Message"] = upsertResult != null ? "Organization saved successfully." : "Failed to save organization.";

            return RedirectToAction("Details", new { @culture = culture, @organizationId = organizationId });
        }

        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/{culture}/usr/organization/setasdefault")]
        public async Task<IActionResult> SetAsDefaultOrganization(string culture, string organizationId)
        {
            var user = await _userManager.GetUserAsync(User);

            user.OrganizationId = organizationId;
            await _userManager.UpdateAsync(user);

            TempData["Message"] = "Default organization set successfully.";

            return RedirectToAction("Details", new { @culture = culture, @organizationId = organizationId });
        }
    }
}
