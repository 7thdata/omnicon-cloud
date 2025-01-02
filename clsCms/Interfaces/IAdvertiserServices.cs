using clsCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface IAdvertiserServices
    {
        Task CreateAdCampaignAsync(AdCampaignModel adCampaign);
        Task<AdCampaignModel> GetAdCampaignAsync(string organizationId, string adCampaignId);
        Task UpdateAdCampaignAsync(AdCampaignModel adCampaign);
        Task DeleteAdCampaignAsync(string organizationId, string adCampaignId);
        Task<PaginationModel<AdCampaignModel>> GetAdCampaignsAsync(string organizationId, string keyword, string sort, int currentPage, int itemsPerPage);


        Task CreateAdGroupAsync(AdGroupModel adGroup);
        Task<AdGroupModel> GetAdGroupAsync(string organizationId, string adGroupId);
        Task UpdateAdGroupAsync(AdGroupModel adGroup);
        Task DeleteAdGroupAsync(string organizationId, string adGroupId);
        Task<PaginationModel<AdGroupModel>> GetAdGroupsAsync(
    string channelId,
    string campaignId,
    string keyword,
    string sort,
    int currentPage,
    int itemsPerPage);


        Task CreateAdGroupAdCreativeMappingAsync(AdGroupAdCreativeMappingModel mapping);
        Task<AdGroupAdCreativeMappingModel> GetAdGroupAdCreativeMappingAsync(string organizationId, string mappingId);
        Task UpdateAdGroupAdCreativeMappingAsync(AdGroupAdCreativeMappingModel mapping);
        Task DeleteAdGroupAdCreativeMappingAsync(string organizationId, string mappingId);
        Task<List<string>> GetAdCreativeIdsByAdGroupAsync(string channelId, string adGroupId);

        Task CreateAdCreativeAsync(AdCreativeModel adCreative);
        Task<AdCreativeModel> GetAdCreativeAsync(string organizationId, string adCreativeId);
        Task UpdateAdCreativeAsync(AdCreativeModel adCreative);
        Task DeleteAdCreativeAsync(string organizationId, string adCreativeId);
        Task<PaginationModel<AdCreativeModel>> GetAdCreativesAsync(
    List<string> adCreativeIds,
    string keyword,
    string sort,
    int currentPage,
    int itemsPerPage);

        Task<PaginationModel<AdCreativeModel>> GetAdCreativesAsync(
    string channelId, string keyword, string sort, int currentPage, int itemsPerPage);
    }
}
