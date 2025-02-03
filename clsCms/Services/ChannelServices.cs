
using clsCms.Interfaces;
using clsCms.Models;
using clsCMs.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace clsCms.Services
{
    public class ChannelServices : IChannelServices
    {
        private readonly ApplicationDbContext _db;

        public ChannelServices(ApplicationDbContext db)
        {
            _db = db;
        }

        #region Channel CRUD

        /// <summary>
        /// Creates a new channel and saves it to the database.
        /// Sets the Created and Updated timestamps to the current UTC time.
        /// </summary>
        /// <param name="channel">The channel model to be created.</param>
        /// <returns>The created ChannelModel object.</returns>
        public async Task<ChannelModel> CreateChannelAsync(ChannelModel channel)
        {
            // Set creation and update timestamps to the current UTC time
            channel.Created = DateTimeOffset.UtcNow;
            channel.Updated = DateTimeOffset.UtcNow;

            // Add the new channel to the database
            _db.Channels.Add(channel);

            var user = await (from u in _db.Users where u.Id == channel.CreatedBy select u).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new KeyNotFoundException($"Create By User with ID {channel.CreatedBy} not found.");
            }

            // Then add the membership for the channel owner
            _db.ChannelMemberships.Add(new ChannelMembershipModel(Guid.NewGuid().ToString(), channel.Id, user.Id, user.Id)
            {
                Accepted = DateTimeOffset.UtcNow,
                IsAccepted = true,
                IsArchived = false,
                IsOwner = true,
                IsEditor = true,
                IsReviewer = true
            });

            await _db.SaveChangesAsync();

            return channel;
        }

        /// <summary>
        /// Get all channels (admin only)
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChannelModel>> AdminGetAllChannels()
        {
            var channels = await (from c in _db.Channels
                                  select c).ToListAsync();
            return channels;
        }

        /// <summary>
        /// Get chanel view of the channel id and user id.
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ChannelViewModel?> GetChannelAsync(string channelId, string? userId = null)
        {
            // Fetch the channel data from the database (without calling GetChannelMemberships inside the query)
            var channel = await (from c in _db.Channels
                                 where c.Id == channelId
                                 select c).FirstOrDefaultAsync();

            if (channel == null)
            {
                return null; // Handle the case where the channel does not exist
            }

            // Now call GetChannelMemberships in-memory after fetching the channel
            var members = GetChannelMemberships(channelId, false);

            // Create the ChannelViewModel and assign the members
            var channelViewModel = new ChannelViewModel(channel, members);

            // Get membership
            if (!string.IsNullOrEmpty(userId))
            {
                var membership = members.FirstOrDefault(m => m.User.Id == userId);
                if (membership == null)
                {
                    return null;
                }

                channelViewModel.Membership = membership;
            }

            return channelViewModel;
        }

        /// <summary>
        /// Get only if the channel is public.
        /// </summary>
        /// <param name="permanName"></param>
        /// <returns></returns>
        public async Task<ChannelViewModel> GetPublicChannelByPermaNameAsync(string permanName)
        {
            // Loggedin User?

            // Fetch the channel data from the database (without calling GetChannelMemberships inside the query)
            var channel = await (from c in _db.Channels
                                 where c.PermaName == permanName && c.IsPublic
                                 select c).FirstOrDefaultAsync();

            if (channel == null)
            {
                return null; // Handle the case where the channel does not exist
            }

            // Now call GetChannelMemberships in-memory after fetching the channel
            var members = GetChannelMemberships(channel.Id, false);

            // Create the ChannelViewModel and assign the members
            var channelViewModel = new ChannelViewModel(channel, members);

            return channelViewModel;
        }


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
        public async Task<PaginationModel<ChannelViewModel>> GetChannelsByUserIdAndOrganizationIdAsync(string organizationId, string userId, string keyword, string sort, int currentPage, int itemsPerPage)
        {
            var channles = from c in _db.Channels
                           join m in _db.ChannelMemberships on c.Id equals m.ChannelId
                           where c.OrganizationId == organizationId &&
                           c.IsArchived == false && m.IsArchived == false && m.IsAccepted &&
                           m.UserId == userId
                           select c;

            // Apply filtering by keyword if provided
            if (!string.IsNullOrEmpty(keyword))
            {
                channles = channles.Where(c => c.PermaName.Contains(keyword) || c.Title.Contains(keyword));
            }

            // Apply sorting based on the provided sort criteria
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort)
                {
                    case "updated-desc":
                        channles = channles.OrderByDescending(c => c.Updated);
                        break;
                    case "updated-asc":
                        channles = channles.OrderBy(c => c.Updated);
                        break;
                    case "title-desc":
                        channles = channles.OrderByDescending(c => c.Title);
                        break;
                    case "title-asc":
                        channles = channles.OrderBy(c => c.Title);
                        break;
                    default:
                        channles = channles.OrderBy(c => c.Title);
                        break;
                }
            }
            else
            {
                channles = channles.OrderBy(c => c.Title);
            }

            // Get the total item count for pagination
            int totalItems = await channles.CountAsync();
            int totalPages = totalItems > 0 ? (totalItems / itemsPerPage) + 1 : 0;

            // Fetch the paginated list of channels
            var channels = await channles.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToListAsync();

            // Now perform the in-memory mapping for members (client-side)
            var channelViewModels = new List<ChannelViewModel>();

            foreach (var channel in channels)
            {
                var members = GetChannelMemberships(channel.Id, false); // This is now evaluated in memory
                channelViewModels.Add(new ChannelViewModel(channel, members));
            }

            // Return the paginated result
            var result = new PaginationModel<ChannelViewModel>(
                channelViewModels,
                currentPage, itemsPerPage, totalItems, totalPages)
            {
                Keyword = keyword,
                Sort = sort
            };

            return result;
        }

        /// <summary>
        /// Updates an existing channel with new data.
        /// </summary>
        /// <param name="channel">The updated ChannelModel object.</param>
        /// <returns>The updated ChannelModel object.</returns>
        public async Task<ChannelModel> UpdateChannelAsync(ChannelModel channel)
        {
            // Fetch the original channel from the database
            var original = await (from o in _db.Channels
                                  where o.Id == channel.Id
                                  select o).FirstOrDefaultAsync();

            if (original == null)
            {
                throw new KeyNotFoundException($"Channel with ID {channel.Id} not found.");
            }

            // Ensure PermaName is unique if it has changed
            if (original.PermaName != channel.PermaName && !IsChannelPermaNameUnique(channel.PermaName))
            {
                throw new InvalidOperationException($"PermaName {channel.PermaName} is already in use.");
            }

            // Update the channel properties
            original.Title = channel.Title;
            original.Description = channel.Description;
            original.IsArchived = channel.IsArchived;
            original.PermaName = channel.PermaName;
            original.Updated = DateTimeOffset.UtcNow;
            original.PublicCss = channel.PublicCss;
            original.DefaultCulture = channel.DefaultCulture;

            // Update the new fields
            original.IsTopPageStaticPage = channel.IsTopPageStaticPage;
            original.TopPagePermaName = channel.TopPagePermaName;

            // Update the channel in the database
            _db.Channels.Update(original);
            await _db.SaveChangesAsync();

            return original;
        }

        /// <summary>
        /// Deletes a channel from the database by its ID.
        /// Throws a KeyNotFoundException if the channel is not found.
        /// </summary>
        /// <param name="channelId">The ID of the channel to delete.</param>
        public async Task DeleteChannelAsync(string channelId)
        {
            // Fetch the channel to delete
            var channel = await (from o in _db.Channels
                                 where o.Id == channelId
                                 select o).FirstOrDefaultAsync();

            // Throw an exception if the channel does not exist
            if (channel == null)
            {
                throw new KeyNotFoundException($"Channel with ID {channelId} not found.");
            }

            // Remove the channel from the database
            _db.Channels.Remove(channel);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a list of all channels, optionally including archived channels.
        /// </summary>
        /// <param name="includeArchived">If true, includes archived channels in the result.</param>
        /// <returns>A list of ChannelModel objects.</returns>
        public async Task<List<ChannelModel>> ListChannelsAsync(bool includeArchived = false)
        {
            // Get all channels, optionally including archived channels
            var channels = from c in _db.Channels select c;

            // Exclude archived channels if requested
            if (!includeArchived)
            {
                channels = channels.Where(c => !c.IsArchived);
            }

            // Return the list of channels
            return await channels.ToListAsync();
        }

        /// <summary>
        /// Checks if a channel's PermaName is unique.
        /// </summary>
        /// <param name="permaName">The PermaName to check for uniqueness.</param>
        /// <returns>True if the PermaName is unique, false otherwise.</returns>
        public bool IsChannelPermaNameUnique(string permaName)
        {
            // Check if any channel exists with the specified PermaName
            return !_db.Channels.Any(o => o.PermaName == permaName);
        }

        /// <summary>
        /// Increments or decrements the article count for a channel.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="add"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<ChannelModel> AddChannelAricleCountAsync(string id, int add)
        {
            var original = (from o in _db.Channels
                            where o.Id == id
                            select o).FirstOrDefault();

            if (original == null)
            {
                throw new KeyNotFoundException($"Channel with ID {id} not found.");
            }

            original.ArticleCount += add;

            _db.Channels.Update(original);
            await _db.SaveChangesAsync();

            return original;

        }

        #endregion


        #region Channel Membership CRUD

        /// <summary>
        /// Creates a new channel membership record and saves it to the database.
        /// </summary>
        /// <param name="channelMembership">The ChannelMembershipModel object representing the new membership.</param>
        /// <returns>The created ChannelMembershipModel object.</returns>
        public async Task<ChannelMembershipModel> CreateChannelMembershipAsync(ChannelMembershipModel channelMembership)
        {
            // Add the new channel membership to the database
            _db.ChannelMemberships.Add(channelMembership);

            // Save changes asynchronously
            await _db.SaveChangesAsync();

            return channelMembership;
        }

        /// <summary>
        /// Updates an existing channel membership record.
        /// Throws ArgumentNullException if the channelMembership is null.
        /// Throws KeyNotFoundException if the membership is not found in the database.
        /// </summary>
        /// <param name="channelMembership">The updated ChannelMembershipModel object.</param>
        /// <returns>The updated ChannelMembershipModel object.</returns>
        public async Task<ChannelMembershipModel> UpdateChannelMembershipAsync(ChannelMembershipModel channelMembership)
        {
            // Ensure that the provided channel membership is not null
            if (channelMembership == null)
            {
                throw new ArgumentNullException(nameof(channelMembership), "Channel membership cannot be null.");
            }

            // Find the original channel membership by its MembershipId
            var original = await (from o in _db.ChannelMemberships
                                  where o.MembershipId == channelMembership.MembershipId
                                  select o).FirstOrDefaultAsync();

            // If the original membership is not found, throw an exception
            if (original == null)
            {
                throw new KeyNotFoundException($"Channel membership with ID {channelMembership.MembershipId} not found.");
            }

            // Update the channel membership fields
            original.Archived = channelMembership.Archived;
            original.Accepted = channelMembership.Accepted;
            original.IsAccepted = channelMembership.IsAccepted;
            original.IsArchived = channelMembership.IsArchived;
            original.IsEditor = channelMembership.IsEditor;
            original.IsOwner = channelMembership.IsOwner;
            original.IsRejected = channelMembership.IsRejected;
            original.IsReviewer = channelMembership.IsReviewer;
            original.Rejected = channelMembership.Rejected;

            // Update the membership in the database
            _db.ChannelMemberships.Update(original);

            // Save changes asynchronously
            await _db.SaveChangesAsync();

            return original;
        }

        /// <summary>
        /// Deletes a channel membership record from the database.
        /// Throws a KeyNotFoundException if the membership is not found.
        /// </summary>
        /// <param name="channelMembershipId">The ID of the channel membership to delete.</param>
        public async Task DeleteChannelMembershipAsync(string channelMembershipId)
        {
            // Find the channel membership by its ID
            var original = await (from o in _db.ChannelMemberships
                                  where o.MembershipId == channelMembershipId
                                  select o).FirstOrDefaultAsync();

            // If the membership is not found, throw an exception
            if (original == null)
            {
                throw new KeyNotFoundException($"Channel membership with ID {channelMembershipId} not found.");
            }

            // Remove the membership from the database
            _db.ChannelMemberships.Remove(original);

            // Save changes asynchronously
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a list of all channel memberships for a specified channel, 
        /// with an option to include archived memberships.
        /// </summary>
        /// <param name="channelId">The ID of the channel.</param>
        /// <param name="includeArchived">If true, includes archived memberships in the result. If false, excludes them.</param>
        /// <returns>A list of ChannelMembershipViewModel objects representing the memberships.</returns>
        public List<ChannelMembershipViewModel> GetChannelMemberships(string channelId, bool includeArchived = false)
        {
            // Fetch the raw channel membership data and user data from the database
            var channelMembersQuery = from m in _db.ChannelMemberships
                                      join u in _db.Users on m.UserId equals u.Id
                                      where m.ChannelId == channelId
                                      select new
                                      {
                                          Membership = m,
                                          User = new ProfileViewModel(u.Id, u.UserName, u.NickName, u.Email, u.IconImage, u.CreatedOn)
                                          {
                                              Introduction = u.Introduction
                                          }
                                      };

            // If includeArchived is false, filter out archived memberships
            if (!includeArchived)
            {
                channelMembersQuery = channelMembersQuery.Where(m => !m.Membership.IsArchived);
            }

            // Execute the query and fetch the results
            var channelMembers = channelMembersQuery.ToList();

            // Perform the in-memory mapping from raw data to ChannelMembershipViewModel
            var result = channelMembers.Select(m => new ChannelMembershipViewModel(
                m.Membership,
                m.User)
            ).ToList();

            return result;
        }


        #endregion

        #region subscription

        /// <summary>
        /// Get subscribers.
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public List<ChannelSubscriberModel> GetSubscribers(string channelId)
        {
            var subscribers = (from s in _db.ChannelSubscribers
                               where s.ChannelId == channelId
                               select s).ToList();

            return subscribers;
        }

        /// <summary>
        /// Subscribe
        /// </summary>
        /// <param name="channeId"></param>
        /// <param name="subseriberId"></param>
        /// <param name="email"></param>
        /// <param name="subscriberLevel"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<ChannelSubscriberModel?> SubscribeToTheChannelAsync(string channeId, string subseriberId,
            string email, string subscriberLevel, string name)
        {

            var original = (from o in _db.ChannelSubscribers
                            where o.ChannelId == channeId && o.Id == subseriberId
                            select o).FirstOrDefault();

            if (original == null)
            {
                _db.ChannelSubscribers.Add(new ChannelSubscriberModel
                {
                    Id = subseriberId,
                    ChannelId = channeId,
                    SubscriberSince = DateTimeOffset.UtcNow,
                    Email = email,
                    SubscriberLevel = subscriberLevel,
                    Name = name
                });

                await _db.SaveChangesAsync();
            }

            return null;

        }

        /// <summary>
        /// Ubsubsriber
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        public async Task<ChannelSubscriberModel?> UnsbsribeToChannelAsync(string channelId, string subscriberId)
        {
            var original = (from o in _db.ChannelSubscribers
                            where o.ChannelId == channelId
                           && o.Id == subscriberId
                            select o).FirstOrDefault();

            if (original == null)
            {
                return null;

            }

            _db.ChannelSubscribers.Remove(original);
            await _db.SaveChangesAsync();

            return original;
        }


        #endregion
    }
}
