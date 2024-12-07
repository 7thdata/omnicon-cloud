using apiCms.Models;
using apiCms.Services;
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
    public class AuthController : ControllerBase
    {
        private readonly UserManager<UserModel> _userManager;
        private readonly IUserServices _userServices;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<UserModel> userManager, IUserServices userServices,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _userServices = userServices;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Handles user login by validating credentials and generating an authentication token.
        /// </summary>
        /// <param name="loginRequest">The request model containing the user's username and password.</param>
        /// <returns>
        /// An ActionResult containing the authentication token and its expiration time if the login is successful.
        /// </returns>
        /// <response code="200">Returns the authentication token and expiration details for a successful login.</response>
        /// <response code="401">Returns Unauthorized if the credentials are invalid.</response>
        [AllowAnonymous]
        [HttpPost("/login")]
        [ProducesResponseType(typeof(LoginResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponseModel>> Login([FromBody] LoginRequestModel loginRequest)
        {
            // Find the user by username.
            var user = await _userManager.FindByNameAsync(loginRequest.Username);

            // Validate the user's password.
            if (user != null && await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                // Generate an authentication token.
                var (token, expiration) = _tokenService.GenerateToken(user);

                // Return the token and expiration time in the response model.
                return Ok(new LoginResponseModel
                {
                    Token = token,
                    Expiration = expiration
                });
            }

            // Return Unauthorized if credentials are invalid.
            return Unauthorized();
        }

        /// <summary>
        /// Retrieves information about the currently authenticated user.
        /// </summary>
        /// <returns>
        /// An ActionResult containing the authenticated user's details in a MeResponseModel.
        /// </returns>
        /// <response code="200">Returns the user details for the authenticated user.</response>
        /// <response code="401">Returns Unauthorized if the user is not authenticated.</response>
        [HttpGet("/me")]
        [ProducesResponseType(typeof(MeResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            // Get the currently authenticated user.
            var user = await _userManager.GetUserAsync(User);

            // If no user is authenticated, return Unauthorized.
            if (user == null)
            {
                return Unauthorized();
            }

            // Fetch the user's detailed view from the service.
            var userView = await _userServices.GetUserByIdAsync(user.Id);

            // Wrap the result in a response model.
            var result = new MeResponseModel
            {
                Me = userView
            };

            // Return a 200 OK response with the user's details.
            return Ok(result);
        }
    }
}
