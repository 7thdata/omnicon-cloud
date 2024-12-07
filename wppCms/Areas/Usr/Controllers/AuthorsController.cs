
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Policy;
using wppCms.Areas.Usr.Models;

namespace wppCms.Areas.Usr.Controllers
{
    [Area("Usr")]
    [Authorize]
    public class AuthorsController : Controller
    {
        private readonly IAuthorServices _authorServices;
        private readonly UserManager<UserModel> _userManager;
        private readonly IChannelServices _channelServices;

        public AuthorsController(IAuthorServices authorServices,
            UserManager<UserModel> userManager, IChannelServices channelServices)
        {
            _authorServices = authorServices;
            _userManager = userManager;
            _channelServices = channelServices;
        }

        [Route("/{culture}/usr/authors")]
        public async Task<IActionResult> Index(string culture)
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrEmpty(user.OrganizationId))
            {
                return RedirectToAction("Index", "Organization", new { culture = culture });
            }

            var authors = await _authorServices.ListAuthorsByUserIdAndOrganizationIdAsync(user.OrganizationId, user.Id, false);

            var channels = (await _channelServices.GetChannelsByUserIdAndOrganizationIdAsync(user.OrganizationId, user.Id, "", "", 1, 1000)).Items;

            var view = new UsrAuthorsIndexViewModel()
            {
                Authors = authors,
                Culture = culture,
                Channels = channels
            };

            return View(view);
        }


        // POST: Create a new author
        [HttpPost]
        [Route("/{culture}/usr/author/create")]
        public async Task<IActionResult> Create(string culture, AuthorModel author)
        {
            author.SnsLinkJsonString = "";
            author.RowKey = Guid.NewGuid().ToString();
            author.PartitionKey = author.ChannelId;

            var user = await _userManager.GetUserAsync(User);

            await _authorServices.CreateAuthorAsync(author);

            return RedirectToAction("Index", new { culture = culture });

        }


        // POST: Edit an existing author
        [HttpPost]
        [Route("/{culture}/usr/author/edit/{authorId}")]
        public async Task<IActionResult> Edit(string culture, string channelId, string authorId,
            string title, string permaName, string profileImageUrl, string text, 
            DateTime publishSince, DateTime publishUntil)
        {
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

            return RedirectToAction("Index", new { culture = culture });
        }

        // POST: Soft delete an author (set IsArchived to true)
        [HttpPost]
        [Route("/{culture}/usr/author/delete/{authorId}")]
        public async Task<IActionResult> Delete(string culture, string authorId)
        {
            var user = await _userManager.GetUserAsync(User);

            var author = await _authorServices.GetAuthorAsync(user.Id, authorId);
            if (author == null)
            {
                return NotFound();
            }

            author.IsArchived = true;

            await _authorServices.UpdateAuthorAsync(author);

            return RedirectToAction("Index", new { culture = culture });
        }
    }
}
