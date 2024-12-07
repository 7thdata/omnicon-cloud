using clsCms.Models;
using Microsoft.AspNetCore.Identity;

namespace apiCms.Services
{
    public interface ITokenService
    {
        (string Token, DateTime Expiration) GenerateToken(UserModel user);
    }
}
