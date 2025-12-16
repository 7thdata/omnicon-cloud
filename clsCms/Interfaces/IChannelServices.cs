using clsCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface IChannelServices
    {
        #region Channel CRUD

        /// <summary>
        /// Creates a new channel and saves it to the database.
        /// </summary>
        /// <param name="channel">The channel model to be created.</param>
        /// <returns>The created ChannelModel object.</returns>
        Task<ChannelModel> CreateChannelAsync(ChannelModel channel);

        /// <summary>
        /// Get channels
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        Task<PaginationModel<ChannelModel>> GetAllChannelsAsync(string organizationId,
      string keyword, string sort, int currentPage, int itemsPerPage);


        /// <summary>
        /// Get channel by id.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ChannelViewModel?> GetChannelAsync(string channelId, string? userId = null);

        /// <summary>
        /// Get only if the channel is public.
        /// </summary>
        /// <param name="permanName"></param>
        /// <returns></returns>
        Task<ChannelViewModel> GetPublicChannelByPermaNameAsync(string permanName);

        /// <summary>
        /// Get ChannelViews by OrganizationId and UserId
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="userId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        Task<PaginationModel<ChannelViewModel>> GetChannelsByUserIdAndOrganizationIdAsync(string organizationId, string userId, string keyword, string sort, int currentPage, int itemsPerPage);

        /// <summary>
        /// Updates an existing channel with new data.
        /// </summary>
        /// <param name="channel">The updated ChannelModel object.</param>
        /// <returns>The updated ChannelModel object.</returns>
        Task<ChannelModel> UpdateChannelAsync(ChannelModel channel);

        /// <summary>
        /// Deletes a channel from the database by its ID.
        /// </summary>
        /// <param name="channelId">The ID of the channel to delete.</param>
        Task DeleteChannelAsync(string channelId);

        /// <summary>
        /// Retrieves a list of all channels, optionally including archived channels.
        /// </summary>
        /// <param name="includeArchived">If true, includes archived channels in the result.</param>
        /// <returns>A list of ChannelModel objects.</returns>
        Task<List<ChannelModel>> ListChannelsAsync(bool includeArchived = false);

        /// <summary>
        /// Checks if a channel's PermaName is unique.
        /// </summary>
        /// <param name="permaName">The PermaName to check for uniqueness.</param>
        /// <returns>True if the PermaName is unique, false otherwise.</returns>
        bool IsChannelPermaNameUnique(string permaName);

        /// <summary>
        /// Adds or subtracts from the article count of a channel.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        Task<ChannelModel> AddChannelAricleCountAsync(string id, int add);

        #endregion


        #region Channel Membership CRUD

        /// <summary>
        /// Creates a new channel membership record and saves it to the database.
        /// </summary>
        /// <param name="channelMembership">The ChannelMembershipModel object representing the new membership.</param>
        /// <returns>The created ChannelMembershipModel object.</returns>
        Task<ChannelMembershipModel> CreateChannelMembershipAsync(ChannelMembershipModel channelMembership);

        /// <summary>
        /// Updates an existing channel membership record.
        /// Throws ArgumentNullException if the channelMembership is null.
        /// Throws KeyNotFoundException if the membership is not found in the database.
        /// </summary>
        /// <param name="channelMembership">The updated ChannelMembershipModel object.</param>
        /// <returns>The updated ChannelMembershipModel object.</returns>
        Task<ChannelMembershipModel> UpdateChannelMembershipAsync(ChannelMembershipModel channelMembership);

        /// <summary>
        /// Deletes a channel membership record from the database.
        /// Throws a KeyNotFoundException if the membership is not found.
        /// </summary>
        /// <param name="channelMembershipId">The ID of the channel membership to delete.</param>
        Task DeleteChannelMembershipAsync(string channelMembershipId);

        /// <summary>
        /// Retrieves a list of all channel memberships for a specified channel, 
        /// with an option to include archived memberships.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="includeArchived">If true, includes archived memberships in the result. If false, excludes them.</param>
        /// <returns>A list of ChannelMembershipViewModel objects representing the memberships.</returns>
        List<ChannelMembershipViewModel> GetChannelMemberships(string channelId, bool includeArchived = false);

        #endregion

        #region manage subscribers

        /// <summary>
        /// Get subscribers.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        List<ChannelSubscriberModel> GetSubscribers(string channelId);

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="channeId"></param>
        /// <param name="subseriberId"></param>
        /// <param name="email"></param>
        /// <param name="subscriberLevel"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Task<ChannelSubscriberModel?> SubscribeToTheChannelAsync(string channeId, string subseriberId,
            string email, string subscriberLevel, string name);

        /// <summary>
        /// Ubsubsriber
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        Task<ChannelSubscriberModel?> UnsbsribeToChannelAsync(string channelId, string subscriberId);


        #endregion
    }
}
