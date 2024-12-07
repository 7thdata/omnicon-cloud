
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using wppCms.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;

namespace clsCms.Areas.Admin.Controllers
{
    [Authorize()]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserServices _userServices;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserModel> _userManager;

        public UsersController(IUserServices userServices, RoleManager<IdentityRole> roleManager, UserManager<UserModel> userManager)
        {
            _userServices = userServices;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays a paginated list of users with filtering and sorting options.
        /// </summary>
        /// <param name="page">The current page number (default is 1).</param>
        /// <param name="itemsPerPage">The number of users to display per page (default is 10).</param>
        /// <param name="keyword">Optional: A search keyword to filter users by username or email.</param>
        /// <param name="sort">Optional: Sorting criteria (e.g., 'username_asc').</param>
        /// <returns>A view displaying the paginated list of users.</returns>
        [Route("/admin/users")]
        public async Task<IActionResult> Index(int page = 1, int itemsPerPage = 10, string keyword = null, string sort = null)
        {
            // Fetch paginated users
            var paginatedUsers = await _userServices.GetUsersAsync(page, itemsPerPage, keyword, sort);

            // Create the view model for the view
            var viewModel = new AdminUsersIndexViewModel
            {
                PaginatedUsers = paginatedUsers,
                Keyword = keyword,
                Sort = sort
            };

            // Return the view with the view model
            return View(viewModel);
        }

        /// <summary>
        /// Displays detailed information about a single user, including roles and available roles.
        /// </summary>
        [Route("/admin/users/{id}")]
        public async Task<IActionResult> Details(string id)
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
                User = user.User,
                Roles = allRoles.Where(r => userRoles.Contains(r.Name)).ToList(),
                AvailableRoles = availableRoles
            };

            return View(viewModel);
        }

        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/admin/users/{id}/switchemailconfirm")]
        public async Task<IActionResult> EmailConfirmOnOff(string id)
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

            return RedirectToAction("Details", new { id = id });

        }

        /// <summary>
        /// Assign a role to a user.
        /// </summary>
        [HttpPost]
        [Route("/admin/users/{userId}/role/addto")]
        public async Task<IActionResult> AddUserRole(string userId, string roleName)
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

            return RedirectToAction("Details", new { id = userId });
        }

        [Route("/admin/users/roles")]
        public IActionResult Roles()
        {
            var roles = _roleManager.Roles;
            // Prepare view model
            var viewModel = new AdminUsersRolesViewModel
            {
                Roles = roles,
                NewRoleName = string.Empty // Placeholder for new role input
            };

            return View(viewModel);
        }

        // Add a new role - POST
        [HttpPost]
        [Route("/admin/users/role/add")]
        public async Task<IActionResult> AddRole(string roleName)
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
            return RedirectToAction("Roles");
        }
    }
}