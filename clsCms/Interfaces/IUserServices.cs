using clsCms.Models;

namespace clsCms.Services
{
    public interface IUserServices
    {
        Task<UserViewModel> GetUserByIdAsync(string userId); // Fetch a user by ID;
        
        /// <summary>
        /// Fetches a paginated list of users along with their roles, allowing filtering by keyword and sorting.
        /// </summary>
        /// <param name="currentPage">The current page to fetch.</param>
        /// <param name="itemsPerPage">The number of users per page.</param>
        /// <param name="keyword">Optional: A search keyword to filter users by username or email.</param>
        /// <param name="sort">Optional: A string to determine sorting order (e.g., 'username_asc').</param>
        /// <returns>A paginated list of users along with their associated roles.</returns>
        Task<PaginationModel<UserViewModel>> GetUsersAsync(int currentPage, int itemsPerPage, string keyword = null, string sort = null);


        /// <summary>
        /// Get organization by id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<OrganizationModel> GetOrganizationByIdAsync(string organizationId);

        /// <summary>
        /// Get organization view by id
        /// </summary>
        /// <returns></returns>
        Task<OrganizationViewModel> GetOrganizationViewByIdAsync(string organizationId);

        /// <summary>
        /// Retrieves a list of organizations the user is a member of, with optional filtering and sorting.
        /// </summary>
        /// <param name="userId">The ID of the user whose organizations are being fetched.</param>
        /// <param name="keyword">An optional keyword for filtering organizations by name or description.</param>
        /// <param name="sort">An optional sort parameter to define the sorting order.</param>
        /// <returns>A list of organizations with their details and members.</returns>
        Task<List<OrganizationViewModel>> GetMyOrganizationsAsync(string userId,
            string? keyword, string? sort);

        /// <summary>
        /// Upsert organization
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Task<OrganizationModel> UpsertOrganizationAsync(OrganizationModel organization);
        
        /// <summary>
        /// Upsert membership
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        Task<OrganizationMemberModel> UpsertOrganizationMemberAsync(OrganizationMemberModel member);

        /// <summary>
        /// Issue login link
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<LoginLinkModel> IssueLoginLinkAsync(string userId);

        /// <summary>
        /// Consume login link
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        Task<bool> ConsumeLoginLinkAsync(string userId, string linkId);

        /// <summary>
        /// Clean expired login link
        /// </summary>
        /// <returns></returns>
        Task CleanExpiredLoginLinkAsync();
    }
}