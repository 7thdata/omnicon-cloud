using apiCms.Models;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace apiCms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class OrganizationController : ControllerBase
    {
        private readonly IUserServices _userServices;
        private readonly UserManager<UserModel> _userManager;

        public OrganizationController(IUserServices userServices, UserManager<UserModel> userManager)
        {

            _userServices = userServices;
            _userManager = userManager;

        }

        /// <summary>
        /// Fetches a list of organizations the current user is a member of, with optional filtering and sorting.
        /// </summary>
        /// <param name="organizationsRequestModel">The request model containing the filtering and sorting options.</param>
        /// <returns>
        /// A list of organizations, including their details and members, wrapped in a response model.
        /// </returns>
        /// <response code="200">Returns the list of organizations.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost("/organizations")]
        [ProducesResponseType(typeof(GetOrganizationsResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrganizations(GetOrganizationsRequestModel organizationsRequestModel)
        {
            // Fetch the current user.
            var user = await _userManager.GetUserAsync(User);

            // Retrieve organizations based on the user's membership, with filtering and sorting applied.
            var organizations = await _userServices.GetMyOrganizationsAsync(
                user.Id,
                organizationsRequestModel.Keyword,
                organizationsRequestModel.Sort
            );

            // Wrap the result in a response model.
            var result = new GetOrganizationsResponseModel
            {
                Organizations = organizations
            };

            // Return the response.
            return Ok(result);
        }

        /// <summary>
        /// Fetches detailed information about a specific organization by its ID.
        /// </summary>
        /// <param name="organizationRequest">The request model containing the organization ID.</param>
        /// <returns>
        /// Detailed information about the organization, wrapped in a response model.
        /// </returns>
        /// <response code="200">Returns the organization details.</response>
        /// <response code="400">If the organization ID is not provided or invalid.</response>
        /// <response code="404">If the organization is not found.</response>
        [HttpPost("/organization")]
        [ProducesResponseType(typeof(GetOrganizationResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrganization(GetOrganizationRequestModel organizationRequest)
        {
            // Validate the organization ID.
            if (string.IsNullOrEmpty(organizationRequest.OrganizationId))
            {
                return BadRequest(new { error = "OrganizationId is required." });
            }

            // Fetch the organization details by ID.
            var organization = await _userServices.GetOrganizationViewByIdAsync(organizationRequest.OrganizationId);

            // Handle the case where the organization is not found.
            if (organization == null)
            {
                return NotFound(new { error = "Organization not found." });
            }

            // Wrap the result in a response model.
            var result = new GetOrganizationResponseModel
            {
                Organization = organization
            };

            // Return the response.
            return Ok(result);
        }
    }
}

