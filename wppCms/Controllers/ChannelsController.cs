using clsCms.Interfaces;
using clsCms.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using wppCms.Models;

namespace wppCms.Controllers
{
    public class ChannelsController : Controller
    {

        private readonly IChannelServices _channelServices;
        private readonly IArticleServices _articleServices; 
        private readonly IAuthorServices _authorServices;

        public ChannelsController(IChannelServices channelServices, 
            IArticleServices articleServices, IAuthorServices authorServices)
        {
            _channelServices = channelServices;
            _articleServices = articleServices;
            _authorServices = authorServices;
        }

        [Route("/{culture}/c/{permaName}")]
        public async Task<IActionResult> Index(string culture, string permaName, string keyword, int currentPage = 1, int itemsPerPage = 100)
        {
            // This is the top page of public channels.
            // User can choose to make their channle public, then can show contents here.

            // Get public channel by perma name.
            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(permaName);

            if(channel == null)
            {
                return NotFound();
            }

            // Append authors on channel
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors;

            // Get list of articles.
            var articles = await _articleServices.SearchArticlesAsync(channel.Channel.Id, keyword, currentPage, itemsPerPage, "");

            var view = new ChannelsIndexViewModel()
            {
                Culture = culture,
                Channel = channel,
                Articles = articles
            };

            return View(view);
        }

        [Route("/{culture}/c/{channelName}/d/{permaName}")]
        public async Task<IActionResult> Details(string culture, string channelName, string permaName)
        {
            var channel = await _channelServices.GetPublicChannelByPermaNameAsync(channelName);
            // Append authors on channel
            var authors = await _authorServices.ListAuthorsByChannelAsync(channel.Channel.Id);
            channel.Authors = authors;

            var article = await _articleServices.GetArticleViewByPermaNameAsync(channel.Channel.Id, permaName);

            var view = new ChannelsDetailsViewModel()
            {
                Culture = culture,
                Article = article,
                Channel = channel
            };

            return View(view);
        }
    }
}
