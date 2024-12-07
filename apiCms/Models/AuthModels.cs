using clsCms.Models;

namespace apiCms.Models
{
    public class LoginRequestModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class MeResponseModel
    {
        public UserViewModel Me { get; set; }
    }

}
