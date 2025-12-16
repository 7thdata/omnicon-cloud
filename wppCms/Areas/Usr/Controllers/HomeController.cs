using Microsoft.AspNetCore.Mvc;
using wppCms.Areas.Usr.Models;
using clsCms.Services;
using clsCms.Models;
using Microsoft.AspNetCore.Identity;
using clsCms.Interfaces;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class HomeController : UsrBaseController
    {
        private readonly IArticleServices _articleServices;
        private readonly IAuthorServices _authorServices;
        private readonly IUserServices _userServices;
        private readonly AppConfigModel _appConfig;

        public HomeController(UserManager<UserModel> userManager,
        IChannelServices channelServices, IArticleServices articleServices,
            IAuthorServices authoerServices, IUserServices userServices,
            IOptions<AppConfigModel> appConfig) : base(userManager, channelServices)
        {
            _articleServices = articleServices;
            _authorServices = authoerServices;
            _userServices = userServices;
            _appConfig = appConfig.Value;
        }

        [Route("/{culture}/usr")]
        public async Task<IActionResult> Index(string culture,  
            string keyword, string sort, string? organizationId, int currentPage = 1, int itemsPerPage = 300)
        {
            // Get user info.
            var user = await GetAuthenticatedUserAsync();
            
            // If organizationId is not set, then retrieve from user's organization Id
            if (string.IsNullOrEmpty(organizationId))
            {
                if (string.IsNullOrEmpty(user.OrganizationId))
                {
                    // If you have no organization preference set
                    // Then show list of organizations that you belong to.
                    return RedirectToAction("Index", "Organization", new { @area = "Usr", @culture = culture });
                }
                
                // Else just set.
                organizationId = user.OrganizationId;
            }

            // You want to make sure you are the member of this organizaitons
            var channels = await _channelServices.GetChannelsByUserIdAndOrganizationIdAsync(organizationId, user.Id, keyword, sort, currentPage, itemsPerPage);

            var view = new UsrHomeIndexViewModel()
            {
                Channels = channels,
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View(view);
        }

        [HttpPost]
        [Route("/{culture}/usr/channel/add")]
        public async Task<IActionResult> AddChannel(ChannelModel channel, string culture)
        {
            var user = await GetAuthenticatedUserAsync();

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
        [Route("/{culture}/usr/channel/{channelId}/update")]
        public async Task<IActionResult> UpdateChannel(string culture, string channelId, string title,
       string permaName, string description, string publicCss, bool isPublic, bool isTopPageStaticPage,
       string? topPagePermaName, string? defaultCulture)
        {
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            if (channel == null)
            {
                // Handle the case where the channel is not found
                return NotFound();
            }

            // Update the channel properties
            channel.Channel.Title = title;
            channel.Channel.PermaName = permaName;
            channel.Channel.Description = description;
            channel.Channel.IsPublic = isPublic;
            channel.Channel.PublicCss = publicCss;
            channel.Channel.DefaultCulture = defaultCulture;

            // Update the new fields
            channel.Channel.IsTopPageStaticPage = isTopPageStaticPage;
            channel.Channel.TopPagePermaName = topPagePermaName;

            // Save the updated channel
            var result = await _channelServices.UpdateChannelAsync(channel.Channel);

            if (result == null)
            {
                // Handle the case where the update fails
                ModelState.AddModelError("", "Failed to update the channel.");
                return View("Edit", channel); // Return the edit view with the current channel data
            }

            // Set a success message in TempData
            TempData["SuccessMessage"] = "Channel updated successfully!";

            // Redirect back to the Channel details page after successful update
            return RedirectToAction("Details", new { culture, channelId });
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
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);

            var folders = await _articleServices.GetFolderFacetsAsync(channel.Channel.Id, true);

            channel.Authors = authors;

            // Build the view model
            var viewModel = new UsrHomeDetailsViewModel
            {
                Channel = channel,
                Culture = culture,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Folders = folders,
            };

            return View(viewModel); // Render the view with channel and articles
        }

        /// <summary>
        /// Edit channel
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        [Route("/{culture}/usr/channel/{channelId}/edit")]
        public async Task<IActionResult> Edit(string culture, string channelId)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var view = new UsrHomeEditViewModel()
            {
                Channel = channel,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Culture = culture
            };

            return View(view);
        }

    }
}
