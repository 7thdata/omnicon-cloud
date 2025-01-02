using Microsoft.AspNetCore.Mvc;
using wppCms.Areas.Usr.Models;
using clsCms.Services;
using clsCms.Models;
using Microsoft.AspNetCore.Identity;
using clsCms.Interfaces;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class HomeController : Controller
    {

        private readonly IArticleServices _articleServices;
        private readonly IChannelServices _channelServices;
        private readonly IAuthorServices _authorServices;
        private readonly UserManager<UserModel> _userManager;
        private readonly AppConfigModel _appConfig;

        public HomeController(IArticleServices articleServices, 
            IChannelServices channelServices, IAuthorServices authoerServices, UserManager<UserModel> userManager,
            IOptions<AppConfigModel> appConfig)
        {
            _articleServices = articleServices;
            _channelServices = channelServices;
            _authorServices = authoerServices;
            _userManager = userManager;
            _appConfig = appConfig.Value;
        }

        [Route("/{culture}/usr")]
        public async Task<IActionResult> Index(string culture, string keyword, string sort, int currentPage = 1, int itemsPerPage = 10)
        {

            // Get the current user's information
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(user.OrganizationId))
            {
                return RedirectToAction("Index","Organization", new { @area="Usr", @culture=culture});
            }

            var view = new UsrHomeIndexViewModel()
            {
                Channels = await _channelServices.GetChannelsByUserIdAndOrganizationIdAsync(user.OrganizationId, user.Id, keyword, sort, currentPage, itemsPerPage),
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/add")]
        public async Task<IActionResult> AddChannel(ChannelModel channel, string culture)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            // Check if PermaName is unique within the user's channels
            var isUnique = _channelServices.IsChannelPermaNameUnique(channel.PermaName);
            if (!isUnique)
            {
                ModelState.AddModelError("PermaName", "The PermaName already exists. Please choose another one.");
            }

            // Call the service to add the channel
            channel.Id = Guid.NewGuid().ToString();
            channel.OrganizationId = user.OrganizationId;
            channel.CreatedBy = user.Id;
            await _channelServices.CreateChannelAsync(channel);

            // Redirect back to the Index page after the channel is added
            return RedirectToAction("Index", new { culture = culture });
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/edit/{id}")]
        public async Task<IActionResult> UpdateChannel(string culture, string id, string title, string permaName, string description, string publicCss, bool isPublic)
        {
           
            // Fetch the channel by its ID
            var channelObject = await _channelServices.GetChannelAsync(id);

            // If the channel is not found, return 404
            if (channelObject.Channel == null)
            {
                return NotFound();
            }

            var channel = channelObject.Channel;

            // Update the channel properties
            channel.Title = title;
            channel.PermaName = permaName;
            channel.Description = description;
            channel.IsPublic = isPublic;
            channel.PublicCss = publicCss;

            // Save the updated channel
            var result = await _channelServices.UpdateChannelAsync(channel);

            // Redirect back to the Channel details page after successful update
            return RedirectToAction("Details", new { culture = culture, channelId = id });
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/delete/{id}")]
        public async Task<IActionResult> DeleteChannel(string culture, string id)
        {
            // Fetch the channel by its ID
            var channelObject = await _channelServices.GetChannelAsync(id);

            // If the channel is not found, return 404
            if (channelObject.Channel == null)
            {
                return NotFound();
            }

            var channel = channelObject.Channel;

            // Set IsArchived to true for soft delete
            channel.IsArchived = true;

            // Update the channel in the database
            var result = await _channelServices.UpdateChannelAsync(channel);

            // Redirect to the channel list or another appropriate page
            return RedirectToAction("Index", new { culture = culture });
        }

        [Route("/{culture}/usr/channel/{channelId}")]
        public async Task<IActionResult> Details(string culture, string channelId, string keyword)
        {
            var userId = _userManager.GetUserId(User);

            // Fetch the channel by channelId
            var channel = await _channelServices.GetChannelAsync(channelId);
            if (channel == null)
            {
                return NotFound();
            }

            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);

            channel.Authors = authors;

            // Build the view model
            var viewModel = new UsrHomeChannelDetailsViewModel
            {
                Channel = channel,
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(viewModel); // Render the view with channel and articles
        }

    }
}
