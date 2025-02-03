using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace wppCms.Areas.Usr.Controllers
{
  
    public class UsrBaseController : Controller
    {
        protected readonly UserManager<UserModel> _userManager;
        protected readonly IChannelServices _channelServices;

        public UsrBaseController(UserManager<UserModel> userManager, IChannelServices channelServices)
        {
            _userManager = userManager;
            _channelServices = channelServices;
        }

        protected async Task<UserModel> GetAuthenticatedUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return user;
        }

        protected async Task<ChannelViewModel> GetChannelAsync(string channelId)
        {
            var channel = await _channelServices.GetChannelAsync(channelId);
            if (channel == null)
            {
                throw new KeyNotFoundException($"Channel with ID {channelId} not found.");
            }

            return channel;
        }
    }
}
