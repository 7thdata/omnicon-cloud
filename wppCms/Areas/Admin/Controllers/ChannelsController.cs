using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wppCms.Areas.Admin.Models;

namespace wppCms.Areas.Admin.Controllers
{
    [Authorize(Roles = "admin")]
    [Area("Admin")]
    public class ChannelsController : Controller
    {
        private AppConfigModel _appConfig;
        private IChannelServices _channelServices;

        public ChannelsController(IOptions<AppConfigModel> appConfig, IChannelServices channelServices)
        {
            _appConfig = appConfig.Value;
            _channelServices = channelServices;
        }

        /// <summary>
        /// Get channels.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="organizationId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/admin/channels")]
        public async Task<IActionResult> Index(string culture, string organizationId, string keyword, string sort, int currentPage = 1, int itemsPerPage = 50)
        {
            var channels = await _channelServices.GetAllChannelsAsync(organizationId, keyword, sort, currentPage, itemsPerPage);

            var view = new AdminChannelsIndexViewModel()
            {
                Channels = channels,
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View(view);
        }

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("/{culture}/admin/channel/{id}")]
        public async Task<IActionResult> Details(string culture, string id)
        {

            var channel = await _channelServices.GetChannelAsync(id);

            var view = new AdminChannelsDetailsViewModel()
            {
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                Channel = channel,
                Culture = culture
            };

            return View(view);

        }

        [Route("/{culture}/admin/channel/{channelId}/member/add")]
        [Route("/{culture}/admin/channel/{channelId}/member/{id}")]
        public async Task<IActionResult> ChannelMember(string culture, string channelId, string id)
        {
            // get channel
            var channel = await _channelServices.GetChannelAsync(channelId);

            if (channel == null)
            {
                return NotFound();
            }

            ChannelMembershipModel channelMember;

            if (string.IsNullOrEmpty(id))
            {
                channelMember = new ChannelMembershipModel(Guid.NewGuid().ToString(), channelId, string.Empty,
                    string.Empty)
                {
                    Accepted = DateTimeOffset.UtcNow,
                    Archived = DateTimeOffset.MinValue,
                    IsAccepted = true,
                    IsArchived = false,
                    InvitedOn = DateTimeOffset.UtcNow,
                    IsEditor = false,
                    IsOwner = true,
                    IsRejected = false,
                    IsReviewer = false,
                    Rejected = DateTimeOffset.MinValue
                };
            }
            else
            {
                channelMember = channel.Members.FirstOrDefault(m => m.Membership.MembershipId == id)?.Membership;

                if (channelMember == null)
                {
                    return NotFound();

                }
            }

            var view = new AdminChannelsChannelMemberViewModel()
            {
                Channel = channel,
                ChannelMembership = channelMember,
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl
            };

            return View(view);
        }

        [HttpPost, AutoValidateAntiforgeryToken]
        [Route("/{culture}/admin/channel/{channelId}/member/upsert")]
        public async Task<IActionResult> UpsertChannelMember(string culture, ChannelMembershipModel channelMembership)
        {
            // Get 
            var channel = await _channelServices.GetChannelAsync(channelMembership.ChannelId);
            if (channel == null)
            {
                return NotFound();

            }

            var membership = channel.Members.FirstOrDefault(m => m.Membership.MembershipId == channelMembership.MembershipId)?.Membership;

            if (membership == null)
            {
                var createResult = await _channelServices.CreateChannelMembershipAsync(channelMembership);
            }
            else
            {
                var updateResult = await _channelServices.UpdateChannelMembershipAsync(channelMembership);
            }

            return RedirectToAction("Details", new { @culture = culture, @id = channelMembership.ChannelId });
        }
    }
}
