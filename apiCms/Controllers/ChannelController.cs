using apiCms.Models;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace apiCms.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ChannelController : ControllerBase
    {
        private readonly IChannelServices _channelServices;
        private readonly UserManager<UserModel> _userManager;

        public ChannelController(IChannelServices channelServices, UserManager<UserModel> userManager)
        {

            _channelServices = channelServices;
            _userManager = userManager;
        }

        /// <summary>
        /// Fetches a list of channels in a specified organization with optional filtering, sorting, and pagination.
        /// </summary>
        /// <param name="requestChannels">
        /// The request model containing the organization ID, keyword for filtering, sorting options, and pagination details.
        /// </param>
        /// <returns>
        /// An ActionResult containing a paginated list of channels wrapped in a GetChannelsResponseModel.
        /// </returns>
        /// <response code="200">Returns the list of channels successfully.</response>
        /// <response code="400">Returns BadRequest if required parameters (e.g., OrganizationId) are missing or invalid.</response>
        /// <response code="401">Returns Unauthorized if the user is not authenticated.</response>
        [HttpPost("/channels")]
        [ProducesResponseType(typeof(GetChannelsResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetChannelsResponseModel>> GetChannels(GetChannelsRequestModel requestChannels)
        {
            // Get the current user.
            var user = await _userManager.GetUserAsync(User);

            // Fetch channels based on the organization ID, user ID, and optional filters/sorting.
            var channels = await _channelServices.GetChannelsByUserIdAndOrganizationIdAsync(
                requestChannels.OrganizationId,
                user.Id,
                requestChannels.Keyword,
                requestChannels.Sort,
                requestChannels.CurrentPage,
                requestChannels.ItemsPerPage
            );

            // Wrap the result in a response model.
            var result = new GetChannelsResponseModel
            {
                Channels = channels
            };

            // Return the response.
            return Ok(result);
        }

        /// <summary>
        /// Fetches detailed information about a specific channel by its ID.
        /// </summary>
        /// <param name="requestChannel">The request model containing the channel ID.</param>
        /// <returns>
        /// Detailed information about the specified channel wrapped in a response model.
        /// </returns>
        /// <response code="200">Returns the channel details.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="400">If the channel ID is missing or invalid.</response>
        /// <response code="404">If the specified channel is not found.</response>
        [HttpPost("/channel")]
        [ProducesResponseType(typeof(GetChannelResponseModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetChannelResponseModel>> GetChannel(GetChannelRequestModel requestChannel)
        {
            // Validate the channel ID.
            if (string.IsNullOrEmpty(requestChannel.ChannelId))
            {
                return BadRequest(new { error = "ChannelId is required." });
            }

            // Fetch the channel details by ID.
            var channel = await _channelServices.GetChannelAsync(requestChannel.ChannelId);

            // Handle the case where the channel is not found.
            if (channel == null)
            {
                return NotFound(new { error = "Channel not found." });
            }

            // Wrap the result in a response model.
            var result = new GetChannelResponseModel
            {
                Channel = channel
            };

            // Return the response.
            return Ok(result);
        }
    }

}
