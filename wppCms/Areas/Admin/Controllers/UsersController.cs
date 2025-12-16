
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using wppCms.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;
using clsCms.Interfaces;
using AspNetCoreGeneratedDocument;

namespace clsCms.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserServices _userServices;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserModel> _userManager;
        private AppConfigModel _appConfig;

        public UsersController(IUserServices userServices, RoleManager<IdentityRole> roleManager,
            UserManager<UserModel> userManager, IOptions<AppConfigModel> appConfig)
        {
            _userServices = userServices;
            _roleManager = roleManager;
            _userManager = userManager;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Displays a paginated list of users with filtering and sorting options.
        /// </summary>
        /// <param name="page">The current page number (default is 1).</param>
        /// <param name="itemsPerPage">The number of users to display per page (default is 10).</param>
        /// <param name="keyword">Optional: A search keyword to filter users by username or email.</param>
        /// <param name="sort">Optional: Sorting criteria (e.g., 'username_asc').</param>
        /// <returns>A view displaying the paginated list of users.</returns>
        [Route("/{culture}/admin/users")]
        public async Task<IActionResult> Index(string culture, string? keyword = null, string? sort = null, int page = 1, int itemsPerPage = 10, bool showOnlyEmailConfirmed = false)
        {
            // Fetch paginated users
            var paginatedUsers = await _userServices.GetUsersAsync(page, itemsPerPage, keyword, sort, showOnlyEmailConfirmed);

            // Create the view model for the view
            var viewModel = new AdminUsersIndexViewModel
            {
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Culture = culture,
                PaginatedUsers = paginatedUsers,
                ShowOnlyEmailConfirmed = showOnlyEmailConfirmed
            };

            // Return the view with the view model
            return View(viewModel);
        }

        /// <summary>
        /// Displays detailed information about a single user, including roles and available roles.
        /// </summary>
        [Route("/{culture}/admin/users/{id}")]
        public async Task<IActionResult> Details(string culture, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userServices.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Get all roles
            var allRoles = _roleManager.Roles.ToList();

            // Get roles already assigned to the user
            var userRoles = await _userManager.GetRolesAsync(user.User);

            // Filter available roles
            var availableRoles = allRoles.Where(r => !userRoles.Contains(r.Name)).ToList();

            var viewModel = new AdminUsersDetailsViewModel
            {
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                User = user.User,
                Roles = allRoles.Where(r => userRoles.Contains(r.Name)).ToList(),
                AvailableRoles = availableRoles
            };

            return View(viewModel);
        }

        /// <summary>
        /// Get organizations
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/admin/organizations")]
        public IActionResult Organizations(string culture, string keyword, string sort, int currentPage = 1, int itemsPerPage = 50)
        {
            var organizations = _userServices.GetOrganizations(keyword, sort, currentPage, itemsPerPage, "");

            var view = new AdminUsersOrganizationsViewModel()
            {
                Organizations = organizations,
                Culture = culture
            };

            return View(view);
        }

        /// <summary>
        /// Get organization by id
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("/{culture}/admin/organization/{id}")]
        public async Task<IActionResult> Organization(string culture, string id)
        {
            var organization = await _userServices.GetOrganizationViewByIdAsync(id);

            var view = new AdminUsersOrganizationViewModel()
            {
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Organization = organization
            };

            return View(view);
        }

        /// <summary>
        /// Get organization member
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="organizationId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("/{culture}/admin/organization/{organizationId}/member/{id}")]
        [Route("/{culture}/admin/organization/{organizationId}/member/add")]
        public async Task<IActionResult> OrganizationMember(string culture, string organizationId, string id)
        {
            // Fetch the organization first (since it's always required)
            var organization = await _userServices.GetOrganizationViewByIdAsync(organizationId);
            if (organization == null)
            {
                return NotFound(); // Organization must exist
            }

            OrganizationMemberModel organizationMember;

            // If id is empty, create a new member
            if (string.IsNullOrEmpty(id))
            {
                organizationMember = new OrganizationMemberModel
                {
                    MemberId = Guid.NewGuid().ToString(),
                    OrganizationId = organizationId,
                    UserId = string.Empty,
                    Role = "Admin",
                    Status = "Active"
                };
            }
            else
            {
                // Find the existing member in the organization
                organizationMember = organization.Members
                    .FirstOrDefault(m => m.Member.MemberId == id)?.Member;

                if (organizationMember == null)
                {
                    return NotFound(); // Member not found in organization
                }
            }

            // Prepare the view model
            var viewModel = new AdminUsersOrganizationMemberViewModel
            {
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                OrganizationMember = organizationMember,
                Organization = organization
            };

            return View(viewModel);
        }

        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/{culture}/admin/organization/{organizationId}/member/upsert")]
        public async Task<IActionResult> UpsertUserToOrganization(string culture, OrganizationMemberModel organizationMember)
        {
            // Get member and see if create or update
            var result = await _userServices.UpsertOrganizationMemberAsync(organizationMember);

            return RedirectToAction("Organization", new { @culture= culture, @id = organizationMember.OrganizationId });
        }

        /// <summary>
        /// Turn on/off email confirmation
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/{culture}/admin/users/{id}/switchemailconfirm")]
        public async Task<IActionResult> EmailConfirmOnOff(string culture, string id)
        {

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (user.EmailConfirmed)
            {
                user.EmailConfirmed = false;
            }
            else
            {
                user.EmailConfirmed = true;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            return RedirectToAction("Details", new { culture, id });

        }

        /// <summary>
        /// Assign a role to a user.
        /// </summary>
        [HttpPost]
        [Route("/{culture}/admin/users/{userId}/role/addto")]
        public async Task<IActionResult> AddUserRole(string culture, string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                ModelState.AddModelError("", "Role does not exist.");
                return RedirectToAction("Details", new { id = userId });
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add role.");
            }

            return RedirectToAction("Details", new { culture, id = userId });
        }

        /// <summary>
        /// Get roles
        /// </summary>
        /// <param name="culture"></param>
        /// <returns></returns>
        [Route("/{culture}/admin/users/roles")]
        public IActionResult Roles(string culture)
        {
            var roles = _roleManager.Roles;
            // Prepare view model
            var viewModel = new AdminUsersRolesViewModel
            {
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Culture = culture,
                Roles = roles,
                NewRoleName = string.Empty // Placeholder for new role input
            };

            return View(viewModel);
        }

        /// <summary>
        /// Add new role
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/{culture}/admin/users/role/add")]
        public async Task<IActionResult> AddRole(string culture, string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var roleExists = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Roles");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error while creating role.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Role already exists.");
                }
            }

            TempData["SuccessMessage"] = "Role has been added.";

            return RedirectToAction("Roles", new { culture });
        }
    }
}