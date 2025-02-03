using Azure.Core;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Mvc;

namespace wppCms.Areas.Advertiser.Controllers
{
    [Area("Advertiser")]
    public class AdServingController : Controller
    {
        private readonly IAdvertiserServices _advertiserServices;
        public AdServingController(IAdvertiserServices advertiserServices)
        {
            _advertiserServices = advertiserServices;
        }

        [HttpPost("/{culture}/ad/{channelId}")]
        public async Task<IActionResult> GetMeAnAd(string culture, string channelId, [FromBody] AdRequestModel request)
        {
            if (string.IsNullOrWhiteSpace(channelId) || request == null)
            {
                return BadRequest(new { message = "Invalid request. Channel ID and request data are required." });
            }

            try
            {
                var result = await _advertiserServices.GetAnAdAsync(new AdRequestModel
                {
                    AdType = request.AdType,
                    CategoryExclude = request.CategoryExclude,
                    CategoryInclude = request.CategoryInclude,
                    ChannelId = channelId,
                    Culture = culture,
                    LocationExclude = request.LocationExclude,
                    LocationInclude = request.LocationInclude,
                    YourCurrentCountry = request.YourCurrentCountry,
                    YourCurrentRegion = request.YourCurrentRegion
                });

                if (result == null)
                {
                    return NotFound(new { message = "No suitable ad found for the given criteria." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging (use your logging framework)
                Console.Error.WriteLine($"Error fetching ad: {ex.Message}");

                return StatusCode(500, new { message = "An error occurred while fetching the ad. Please try again later." });
            }
        }
    }
}
