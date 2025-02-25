﻿using Azure;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using wppCms.Areas.Advertiser.Models;

namespace wppCms.Areas.Advertiser.Controllers
{
    [Area("Advertiser")]
    [Authorize()]
    public class HomeController : Controller
    {
        private readonly IAdvertiserServices _advertiserServices;
        private readonly IChannelServices _channelServices;
        private readonly ILogger<HomeController> _logger;
        private readonly AppConfigModel _appConfig;

        public HomeController(IAdvertiserServices advertiserServices,
            IChannelServices channelServices,
            ILogger<HomeController> logger, IOptions<AppConfigModel> appConfig)
        {
            _advertiserServices = advertiserServices;
            _channelServices = channelServices;
            _logger = logger;
            _appConfig = appConfig.Value;
        }

        /// <summary>
        /// Show list of campaigns.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/ads/organization/{channelId}")]
        public async Task<IActionResult> Index(string culture, string channelId, string keyword, string sort, int currentPage = 1, int itemsPerPage = 50)
        {
            var campaigns = await _advertiserServices.GetAdCampaignsAsync(channelId, keyword, sort, currentPage, itemsPerPage);

            // Fetch the channel by channelId
            var channel = await _channelServices.GetChannelAsync(channelId);
            if (channel == null)
            {
                return NotFound();
            }

            var view = new AdvertiserHomeIndexViewModel()
            {
                Ads = campaigns,
                Culture = culture,
                Channel = channel,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(view);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("/{culture}/ads/organization/{channelId}/campaign/create")]
        public async Task<IActionResult> CreateCampaign(string culture, string channelId, AdCampaignModel model)
        {
            // Assign PartitionKey (e.g., organization ID) and RowKey (e.g., campaign ID) dynamically
            model.PartitionKey = channelId;
            model.RowKey = Guid.NewGuid().ToString(); // Generate unique ID for the campaign

            // Optionally set default values for fields not provided
            model.Timestamp = DateTimeOffset.UtcNow;
            model.ETag = ETag.All;

            // Save the campaign using the service
            await _advertiserServices.CreateAdCampaignAsync(model);

            // Set a success message in TempData
            TempData["SuccessMessage"] = "Campaign created successfully!";

            // Redirect back to the Index page
            return RedirectToAction("Index", new { culture, channelId });
        }

        /// <summary>
        /// Show detail of campaign, and list of ad groups.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="channelId"></param>
        /// <param name="campaignId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/ads/organization/{channelId}/campaign/{campaignId}")]
        public async Task<IActionResult> CampaignDetails(
            string culture,
            string channelId,
            string campaignId,
            string keyword,
            string sort,
            int currentPage = 1,
            int itemsPerPage = 10)
        {
            // Fetch the channel by channelId
            var channel = await _channelServices.GetChannelAsync(channelId);
            if (channel == null)
            {
                return NotFound();
            }

            // Fetch the campaign details by campaignId
            var campaign = await _advertiserServices.GetAdCampaignAsync(channelId, campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            // Fetch paginated ad groups
            var adGroups = await _advertiserServices.GetAdGroupsAsync(
                channelId,
                campaignId,
                keyword,
                sort,
                currentPage,
                itemsPerPage);

            // Create view model
            var viewModel = new AdvertiserHomeCampaignDetailsViewModel
            {
                Channel = channel,
                AdCampaign = campaign,
                AdGroups = adGroups,
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/{culture}/ads/organization/{channelId}/campaign/{campaignId}/adgroup/create")]
        public async Task<IActionResult> CreateAdGroup(
    string culture,
    string channelId,
    string campaignId,
    AdGroupModel model)
        {

            // Set PartitionKey and RowKey
            model.PartitionKey = channelId; // Organization/Channel ID
            model.RowKey = Guid.NewGuid().ToString(); // Unique ID for the ad group
            model.AdCampaignId = campaignId;

            // Optionally set default values for fields not provided
            model.Timestamp = DateTimeOffset.UtcNow;
            model.ETag = ETag.All;

            // Save the ad group using the service
            await _advertiserServices.CreateAdGroupAsync(model);

            // Set a success message in TempData
            TempData["SuccessMessage"] = "Ad group created successfully!";

            // Redirect back to the campaign details page
            return RedirectToAction("CampaignDetails", new { culture, channelId, campaignId });
        }

        /// <summary>
        /// See details of ad groups and ad creative in it.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="campaignId"></param>
        /// <param name="adgroupId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/ads/organization/{channelId}/campaign/{campaignId}/adgroup/{adGroupId}")]
        public async Task<IActionResult> AdGroupDetails(string culture, string channelId,
            string campaignId, string adGroupId,
            string keyword, string sort, int currentPage, int itemsPerPage)
        {
            // Fetch the channel by channelId
            var channel = await _channelServices.GetChannelAsync(channelId);
            if (channel == null)
            {
                return NotFound();
            }

            // Fetch the campaign details by campaignId
            var campaign = await _advertiserServices.GetAdCampaignAsync(channelId, campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            // Fetch the campaign details by campaignId
            var adGroup = await _advertiserServices.GetAdGroupAsync(channelId, adGroupId);
            if (adGroup == null)
            {
                return NotFound();
            }

            // Get pagination of ads

            var view = new AdvertiserHomeAdGroupDetailsViewModel()
            {
                Culture = culture,
                Channel = channel,
                AdCampaign = campaign,
                AdGroup = adGroup,

                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                GaTagId = _appConfig.Ga.TagId
            };


            // Fetch ad creative IDs from the mapping table
            var adCreativeIds = await _advertiserServices.GetAdCreativeIdsByAdGroupAsync(channelId, adGroupId);
            var fullAdCreativeList = await _advertiserServices.GetAdCreativesAsync(channelId, "", "", 1000);

            if (adCreativeIds.Count > 0)
            {
                // Fetch paginated ad creatives based on the IDs
                var adCreatives = await _advertiserServices.GetAdCreativesFromIdsAsync(channelId, adCreativeIds, keyword, sort, 1000);

                view.AddedCreatives = adCreatives;

                // Use HashSet for faster lookups
                var adCreativeKeys = new HashSet<string>(adCreatives.Select(ac => ac.RowKey));

                fullAdCreativeList = fullAdCreativeList
                    .Where(ad => !adCreativeKeys.Contains(ad.RowKey))
                    .ToList();
            }

            view.AddableCreatives = fullAdCreativeList;

            return View(view);
        }


        [HttpPost("/{culture}/ads/organization/{channelId}/campaign/{campaignId}/adgroup/{adGroupId}/addad")]
        public async Task<IActionResult> AddAdToAdGroup(string culture, string channelId, string campaignId,
    string adGroupId, string adId)
        {
            try
            {
                // Create the mapping between AdGroup and AdCreative
                await _advertiserServices.CreateAdGroupAdCreativeMappingAsync(new AdGroupAdCreativeMappingModel()
                {
                    AdCreativeId = adId,
                    AdGroupId = adGroupId,
                    PartitionKey = channelId,
                    RowKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTimeOffset.UtcNow,
                });

                // Set TempData success message
                TempData["SuccessMessage"] = "Ad successfully added to the ad group.";
            }
            catch (Exception ex)
            {
                // Set TempData error message in case of an exception
                TempData["ErrorMessage"] = $"An error occurred while adding the ad: {ex.Message}";
            }

            return RedirectToAction("AdGroupDetails", new { culture, channelId, campaignId, adGroupId });
        }

        /// <summary>
        /// See list of creatives across.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        [Route("/{culture}/ad/organization/{channelId}/creatives")]
        public async Task<IActionResult> Ads(string culture, string channelId,
            string keyword, string sort, int currentPage = 1, int itemsPerPage = 1000)
        {
            // Fetch the channel details
            var channel = await _channelServices.GetChannelAsync(channelId);
            if (channel == null)
            {
                return NotFound();
            }

            // Fetch the list of ad creatives with pagination
            var adCreatives = await _advertiserServices.GetAdCreativesAsync(
                channelId, keyword, sort, itemsPerPage);

            // Create the view model
            var viewModel = new AdvertiserHomeAdsViewModel
            {
                Channel = channel,
                AdCreative = adCreatives,
                Culture = culture,
                FontAwsomeUrl = _appConfig.FontAwsome.KitUrl,
                GaTagId = _appConfig.Ga.TagId
            };

            return View(viewModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("/{culture}/ad/organization/{channelId}/creatives/create")]
        public async Task<IActionResult> CreateAdCreative(string culture, string channelId, AdCreativeModel model)
        {


            if (!model.Validate(out var validationError))
            {
                TempData["ErrorMessage"] = validationError;
                return View(model);
            }

            model.PartitionKey = channelId;
            model.RowKey = Guid.NewGuid().ToString();
            model.Timestamp = DateTimeOffset.UtcNow;
            model.ETag = ETag.All;

            await _advertiserServices.CreateAdCreativeAsync(model);

            TempData["SuccessMessage"] = "Ad Creative created successfully!";
            return RedirectToAction("Ads", new { culture, channelId });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("/{culture}/ad/organization/{channelId}/creatives/{creativeId}/edit")]
        public async Task<IActionResult> EditAdCreative(string culture, string channelId, 
            string creativeId,
            AdCreativeModel model)
        {

            model.PartitionKey = channelId;
            model.RowKey = creativeId;
            model.Timestamp = DateTimeOffset.UtcNow;
            model.ETag = ETag.All;

            await _advertiserServices.CreateAdCreativeAsync(model);

            TempData["SuccessMessage"] = "Ad Creative created successfully!";
            return RedirectToAction("Ads", new { culture, channelId });
        }


        /// <summary>
        /// See details of creative.
        /// </summary>
        /// <param name="culture"></param>
        /// <param name="creativeId"></param>
        /// <returns></returns>
        public IActionResult AdDetails(string culture, string creativeId)
        {
            return View();
        }
    }

}

