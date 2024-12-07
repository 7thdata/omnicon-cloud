using clsCms.Models;

namespace apiCms.Models
{

    /// <summary>
    /// Request model for fetching a list of channels in an organization with optional filtering and sorting.
    /// </summary>
    public class GetChannelsRequestModel
    {
        /// <summary>
        /// The ID of the organization to fetch channels for.
        /// </summary>
        /// <example>org-123</example>
        public string OrganizationId { get; set; }

        /// <summary>
        /// An optional keyword to filter channels by name or description.
        /// </summary>
        /// <example>technology</example>
        public string Keyword { get; set; }

        /// <summary>
        /// An optional sort parameter to order the channels.
        /// Valid values: name_asc, name_desc, created_asc, created_desc.
        /// </summary>
        /// <example>name_asc</example>
        public string Sort { get; set; }

        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
    }

   

    /// <summary>
    /// Request model for fetching details of a specific channel in an organization.
    /// </summary>
    public class GetChannelRequestModel
    {
        /// <summary>
        /// The ID of the organization the channel belongs to.
        /// </summary>
        /// <example>org-123</example>
        public string OrganizationId { get; set; }

        /// <summary>
        /// The ID of the channel to fetch details for.
        /// </summary>
        /// <example>channel-456</example>
        public string ChannelId { get; set; }
    }

    /// <summary>
    /// Response model for fetching a list of channels in an organization.
    /// </summary>
    public class GetChannelsResponseModel
    {
        /// <summary>
        /// A list of channels matching the request criteria.
        /// </summary>
        public PaginationModel<ChannelViewModel> Channels { get; set; }
    }

    /// <summary>
    /// Response model for fetching details of a specific channel.
    /// </summary>
    public class GetChannelResponseModel
    {
        /// <summary>
        /// The detailed information about the channel.
        /// </summary>
        public ChannelViewModel Channel { get; set; }
    }
}
