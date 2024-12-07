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

        [AllowAnonymous]
        [HttpPost("/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, loginRequest.Password))
            {
                var (token, expiration) = _tokenService.GenerateToken(user);
                return Ok(new LoginResponseModel
                {
                    Token = token,
                    Expiration = expiration
                });
            }

            return Unauthorized();
        }

        [HttpGet("/me")]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var userView = await _userServices.GetUserByIdAsync(user.Id);

            var result = new MeResponseModel()
            {
                Me = userView
            };

            return Ok(result);
        }
    }
}
