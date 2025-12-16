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
            if (user == null) return NotFound();

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

        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/{culture}/usr/organization/add-member")]
        public async Task<IActionResult> AddMember(
    string culture,
    string organizationId,
    string email,
    string role)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // -----------------------------------
            // Resolve user by email
            // -----------------------------------
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["Message"] = "User with this email was not found.";
                return RedirectToAction("Details", new { culture, organizationId });
            }

            // -----------------------------------
            // Prevent duplicate membership
            // -----------------------------------
            var organization = await _userServices.GetOrganizationViewByIdAsync(organizationId);
            if (organization == null) return NotFound();

            if (organization.Members.Any(m => m.Member.UserId == user.Id))
            {
                TempData["Message"] = "User is already a member of this organization.";
                return RedirectToAction("Details", new { culture, organizationId });
            }

            // -----------------------------------
            // Create membership
            // -----------------------------------
            var member = new OrganizationMemberModel
            {
                MemberId = Guid.NewGuid().ToString(),
                OrganizationId = organizationId,
                UserId = user.Id,
                Role = string.IsNullOrEmpty(role) ? "Member" : role,
                Status = "Active",
                Created = DateTimeOffset.UtcNow,
                Updated = DateTimeOffset.UtcNow,
                Joined = DateTimeOffset.UtcNow
            };

            await _userServices.UpsertOrganizationMemberAsync(member);

            TempData["Message"] = "Member added successfully.";

            return RedirectToAction("Details", new { culture, organizationId });
        }
    }
}
