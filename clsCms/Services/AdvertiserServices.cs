using Azure;
using Azure.Data.Tables;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace clsCms.Services
{
    public class AdvertiserServices : IAdvertiserServices
    {
        private readonly TableClient _adCampaignTable;
        private readonly TableClient _adGroupTable;
        private readonly TableClient _adGroupAdCreativeMappingTable;
        private readonly TableClient _adCreativeTable;
        private readonly AppConfigModel _appConfig;

        // Hardcoded table names
        private const string AdCampaignsTableName = "AdCampaignsTable";
        private const string AdGroupsTableName = "AdGroupsTable";
        private const string AdCreativeMappingTableName = "AdCreativeMappingTable";
        private const string AdCreativeTableName = "AdCreativesTable";

        public AdvertiserServices(IOptions<AppConfigModel> appConfig)
        {
            _appConfig = appConfig.Value;

            var serviceClient = new TableServiceClient(
                 new Uri(_appConfig.AzureStorage.StorageTableUri),
                 new TableSharedKeyCredential(
                     _appConfig.AzureStorage.StorageAccountName,
                     _appConfig.AzureStorage.StorageAccountKey
                 )
             );

            _adCampaignTable = serviceClient.GetTableClient(AdCampaignsTableName);
            _adGroupTable = serviceClient.GetTableClient(AdGroupsTableName);
            _adGroupAdCreativeMappingTable = serviceClient.GetTableClient(AdCreativeMappingTableName);
            _adCreativeTable = serviceClient.GetTableClient(AdCreativeTableName);
            
        }

        #region AdCampaign CRUD

        public async Task CreateAdCampaignAsync(AdCampaignModel adCampaign)
        {
            await _adCampaignTable.CreateIfNotExistsAsync();
            await _adCampaignTable.AddEntityAsync(adCampaign);
        }

        public async Task<AdCampaignModel> GetAdCampaignAsync(string organizationId, string adCampaignId)
        {
            return await _adCampaignTable.GetEntityAsync<AdCampaignModel>(organizationId, adCampaignId);
        }

        public async Task UpdateAdCampaignAsync(AdCampaignModel adCampaign)
        {
            await _adCampaignTable.UpdateEntityAsync(adCampaign, adCampaign.ETag);
        }

        public async Task DeleteAdCampaignAsync(string organizationId, string adCampaignId)
        {
            await _adCampaignTable.DeleteEntityAsync(organizationId, adCampaignId);
        }

        public async Task<PaginationModel<AdCampaignModel>> GetAdCampaignsAsync(string organizationId, string keyword, string sort, int currentPage, int itemsPerPage)
        {
            await _adCampaignTable.CreateIfNotExistsAsync();

            var query = _adCampaignTable.QueryAsync<AdCampaignModel>(c => c.PartitionKey == organizationId);
            var result = new List<AdCampaignModel>();

            await foreach (var campaign in query)
            {
                if (string.IsNullOrEmpty(keyword) || campaign.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(campaign);
                }
            }

            if (sort == "Name")
            {
                result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
            }

            var totalItems = result.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var skip = (currentPage - 1) * itemsPerPage;
            var items = result.Skip(skip).Take(itemsPerPage).ToList();

            return new PaginationModel<AdCampaignModel>(
                items,
                currentPage,
                itemsPerPage,
                totalItems,
                totalPages
            )
            {
                Keyword = keyword,
                Sort = sort
            };
        }

        #endregion

        #region AdGroup CRUD

        public async Task CreateAdGroupAsync(AdGroupModel adGroup)
        {
            await _adGroupTable.CreateIfNotExistsAsync();
            await _adGroupTable.AddEntityAsync(adGroup);
        }

        public async Task<AdGroupModel> GetAdGroupAsync(string organizationId, string adGroupId)
        {
            return await _adGroupTable.GetEntityAsync<AdGroupModel>(organizationId, adGroupId);
        }

        public async Task UpdateAdGroupAsync(AdGroupModel adGroup)
        {
            await _adGroupTable.UpdateEntityAsync(adGroup, adGroup.ETag);
        }

        public async Task DeleteAdGroupAsync(string organizationId, string adGroupId)
        {
            await _adGroupTable.DeleteEntityAsync(organizationId, adGroupId);
        }

        public async Task<PaginationModel<AdGroupModel>> GetAdGroupsAsync(
    string channelId,
    string campaignId,
    string keyword,
    string sort,
    int currentPage,
    int itemsPerPage)
        {
            // Ensure the AdGroup table exists
            await _adGroupTable.CreateIfNotExistsAsync();

            // Query the ad groups by channel and campaign
            var query = _adGroupTable.QueryAsync<AdGroupModel>(
                adGroup => adGroup.PartitionKey == channelId && adGroup.AdCampaignId == campaignId);

            var result = new List<AdGroupModel>();

            // Filter by keyword
            await foreach (var adGroup in query)
            {
                if (string.IsNullOrEmpty(keyword) || adGroup.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(adGroup);
                }
            }

            // Sort the results
            switch (sort?.ToLower())
            {
                case "name":
                    result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                    break;
                case "bid":
                    result.Sort((x, y) => x.Bid.CompareTo(y.Bid));
                    break;
            }

            // Calculate pagination
            var totalItems = result.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var paginatedItems = result
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            // Return a pagination model
            return new PaginationModel<AdGroupModel>(
                paginatedItems,
                currentPage,
                itemsPerPage,
                totalItems,
                totalPages)
            {
                Keyword = keyword,
                Sort = sort
            };
        }

        #endregion

        #region AdGroupAdCreativeMapping CRUD

        public async Task CreateAdGroupAdCreativeMappingAsync(AdGroupAdCreativeMappingModel mapping)
        {
            await _adGroupAdCreativeMappingTable.CreateIfNotExistsAsync();
            await _adGroupAdCreativeMappingTable.AddEntityAsync(mapping);
        }

        public async Task<AdGroupAdCreativeMappingModel> GetAdGroupAdCreativeMappingAsync(string organizationId, string mappingId)
        {
            return await _adGroupAdCreativeMappingTable.GetEntityAsync<AdGroupAdCreativeMappingModel>(organizationId, mappingId);
        }

        public async Task UpdateAdGroupAdCreativeMappingAsync(AdGroupAdCreativeMappingModel mapping)
        {
            await _adGroupAdCreativeMappingTable.UpdateEntityAsync(mapping, mapping.ETag);
        }

        public async Task DeleteAdGroupAdCreativeMappingAsync(string organizationId, string mappingId)
        {
            await _adGroupAdCreativeMappingTable.DeleteEntityAsync(organizationId, mappingId);
        }

        public async Task<List<string>> GetAdCreativeIdsByAdGroupAsync(string channelId, string adGroupId)
        {
            var query = _adGroupAdCreativeMappingTable.QueryAsync<AdGroupAdCreativeMappingModel>(
                m => m.PartitionKey == channelId && m.AdGroupId == adGroupId);

            var adCreativeIds = new List<string>();
            await foreach (var mapping in query)
            {
                adCreativeIds.Add(mapping.AdCreativeId);
            }

            return adCreativeIds;
        }

        #endregion

        #region AdCreative CRUD

        public async Task<PaginationModel<AdCreativeModel>> GetAdCreativesAsync(
    List<string> adCreativeIds,
    string keyword,
    string sort,
    int currentPage,
    int itemsPerPage)
        {
            // Ensure the AdGroup table exists
            await _adCreativeTable.CreateIfNotExistsAsync();

            var query = _adCreativeTable.QueryAsync<AdCreativeModel>(
                creative => adCreativeIds.Contains(creative.RowKey));

            var result = new List<AdCreativeModel>();

            await foreach (var creative in query)
            {
                if (string.IsNullOrEmpty(keyword) || creative.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(creative);
                }
            }

            // Sorting logic
            switch (sort?.ToLower())
            {
                case "name":
                    result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                    break;
                case "type":
                    result.Sort((x, y) => string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase));
                    break;
            }

            // Pagination
            var totalItems = result.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var paginatedItems = result
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return new PaginationModel<AdCreativeModel>(paginatedItems, currentPage, itemsPerPage, totalItems, totalPages)
            {
                Keyword = keyword,
                Sort = sort
            };
        }

        public async Task CreateAdCreativeAsync(AdCreativeModel adCreative)
        {
            await _adCreativeTable.CreateIfNotExistsAsync();
            await _adCreativeTable.AddEntityAsync(adCreative);
        }

        public async Task<AdCreativeModel> GetAdCreativeAsync(string organizationId, string adCreativeId)
        {
            return await _adCreativeTable.GetEntityAsync<AdCreativeModel>(organizationId, adCreativeId);
        }

        public async Task UpdateAdCreativeAsync(AdCreativeModel adCreative)
        {
            await _adCreativeTable.UpdateEntityAsync(adCreative, adCreative.ETag);
        }

        public async Task DeleteAdCreativeAsync(string organizationId, string adCreativeId)
        {
            await _adCreativeTable.DeleteEntityAsync(organizationId, adCreativeId);
        }

        public async Task<PaginationModel<AdCreativeModel>> GetAdCreativesAsync(
    string channelId, string keyword, string sort, int currentPage, int itemsPerPage)
        {
            // Ensure the AdGroup table exists
            await _adCreativeTable.CreateIfNotExistsAsync();

            // Query the ad creatives by PartitionKey (channel ID)
            var query = _adCreativeTable.QueryAsync<AdCreativeModel>(creative => creative.PartitionKey == channelId);

            var result = new List<AdCreativeModel>();

            await foreach (var creative in query)
            {
                if (string.IsNullOrEmpty(keyword) || creative.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(creative);
                }
            }

            // Apply sorting
            switch (sort?.ToLower())
            {
                case "name":
                    result.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
                    break;
                case "type":
                    result.Sort((x, y) => string.Compare(x.Type, y.Type, StringComparison.OrdinalIgnoreCase));
                    break;
            }

            // Pagination logic
            var totalItems = result.Count;
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var paginatedItems = result
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return new PaginationModel<AdCreativeModel>(paginatedItems, currentPage, itemsPerPage, totalItems, totalPages)
            {
                Keyword = keyword,
                Sort = sort
            };
        }
        #endregion
    }
}
