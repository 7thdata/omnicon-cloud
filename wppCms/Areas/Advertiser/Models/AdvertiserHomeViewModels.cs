using clsCms.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using wppCms.Models;

namespace wppCms.Areas.Advertiser.Models
{
    public class AdvertiserHomeViewModels
    {
    }

    /// <summary>
    /// List of campaigns
    /// </summary>
    public class AdvertiserHomeIndexViewModel : PageViewModel
    {
        public PaginationModel<AdCampaignModel> Ads { get; set; }
        public ChannelViewModel Channel { get; set; }
    }

    /// <summary>
    /// Campaign and list of ad groups.
    /// </summary>
    public class AdvertiserHomeCampaignDetailsViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public AdCampaignModel AdCampaign { get; set; }
        public PaginationModel<AdGroupModel> AdGroups { get; set; }
    }

    /// <summary>
    /// Campaign, AdGroup and list of Ad Creatives
    /// </summary>
    public class AdvertiserHomeAdGroupDetailsViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public AdCampaignModel AdCampaign { get; set; }
        public AdGroupModel AdGroup { get; set; }
        public PaginationModel<AdCreativeModel> AdCreative { get; set; }
    }

    /// <summary>
    /// Show list of Ad Creatives.
    /// </summary>
    public class AdvertiserHomeAdsViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }
        public PaginationModel<AdCreativeModel> AdCreative { get; set; }
    }

    /// <summary>
    /// Show details of Ad Creative.
    /// </summary>
    public class AdvertiserHomeAdDetailsViewModel : PageViewModel
    {
        public AdCreativeModel AdCreative { get; set; }

        public PaginationModel<AdCampaignModel> AdCampaigns { get; set; }
    }
}
