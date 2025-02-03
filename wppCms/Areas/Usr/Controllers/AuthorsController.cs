
using clsCms.Interfaces;
using clsCms.Models;
using clsCms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Security.Policy;
using System.Threading.Channels;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class AuthorsController : UsrBaseController
    {
        private readonly IAuthorServices _authorServices;
        private readonly AppConfigModel _appConfig;

        public AuthorsController(UserManager<UserModel> userManager,
        IChannelServices channelServices,IAuthorServices authorServices,
            IOptions<AppConfigModel> appConfig) : base(userManager, channelServices)
        {
            _authorServices = authorServices;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Show list of authors for the channel.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        [Route("/{culture}/usr/channel/{channelId}/authors")]
        public async Task<IActionResult> Index(string culture, string channelId)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id, false);

            var view = new UsrAuthorsIndexViewModel()
            {
                Authors = authors,
                Culture = culture,
                Channel = channel,
                GaTagId = _appConfig.Ga.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View(view);
        }

        /// <summary>
        /// Show details of the author.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        [Route("/{culture}/usr/channel/{channelId}/author/{authorId}")]

        public async Task<IActionResult> Details(string culture, string channelId, string authorId)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            var author = await _authorServices.GetAuthorAsync(channelId, authorId);
            if (author == null)
            {
                return NotFound();
            }

            var view = new UsrAuthorsDetailsViewModel()
            {
                Author = author,
                Channel = channel,
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View(view);
        }

        // POST: Create a new author
        [HttpPost]
        [Route("/{culture}/usr/author/create")]
        public async Task<IActionResult> Create(string culture, AuthorModel author)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(author.ChannelId);

            author.SnsLinkJsonString = "";
            author.RowKey = Guid.NewGuid().ToString();
            author.PartitionKey = author.ChannelId;

            await _authorServices.CreateAuthorAsync(author);

            // Set success message
            TempData["Message"] = $"Author '{author.Title}' has been successfully created.";

            return RedirectToAction("Index", new { culture, @channelId = author.ChannelId });

        }

        // POST: Edit an existing author
        [HttpPost]
        [Route("/{culture}/usr/channel/{channelId}/author/edit/{authorId}")]
        public async Task<IActionResult> Edit(string culture, string channelId, string authorId,
            string title, string permaName, string profileImageUrl, string text,
            DateTime publishSince, DateTime publishUntil)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();
            var channel = await GetChannelAsync(channelId);

            // Get original 
            var author = await _authorServices.GetAuthorAsync(channelId, authorId);

            if (author == null)
            {
                return NotFound();
            }

            author.Title = title;

            if (author.PermaName != permaName)
            {
                // You want to check and make sure the permaName is unique
                author.PermaName = permaName;
            }

            author.ProfileImageUrl = profileImageUrl;
            author.Text = text;
            author.PublishSince = publishSince;
            author.PublishUntil = publishUntil;

            await _authorServices.UpdateAuthorAsync(author);

            TempData["Message"] = "Author updated successfully";

            return RedirectToAction("Details", new { culture = culture, authorId = authorId, channelId = channelId });
        }

        // POST: Soft delete an author (set IsArchived to true)
        [HttpPost]
        [Route("/{culture}/usr/author/delete/{authorId}")]
        public async Task<IActionResult> Delete(string culture, string authorId)
        {
            // Get the current user and channel.
            var user = await GetAuthenticatedUserAsync();

            var author = await _authorServices.GetAuthorAsync(user.Id, authorId);
            if (author == null)
            {
                return NotFound();
            }

            var channel = await GetChannelAsync(author.ChannelId);

            author.IsArchived = true;

            await _authorServices.UpdateAuthorAsync(author);

            return RedirectToAction("Index", new { culture = culture });
        }
    }
}
