using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using clsCms.Models;
using clsCMs.Data;
using Microsoft.EntityFrameworkCore;

namespace clsCms.Services
{
    public class UserServices : IUserServices
    {
        private readonly ApplicationDbContext _context;

        public UserServices(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user by id (admin use)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserViewModel> GetUserByIdAsync(string userId)
        {
            // Fetch the user and roles from the database
            var user = await (from u in _context.Users
                              where u.Id == userId
                              select new UserViewModel
                              {
                                  User = new UserModel
                                  {
                                      Id = u.Id,
                                      UserName = u.UserName,
                                      Email = u.Email,
                                      EmailConfirmed = u.EmailConfirmed
                                  },
                                  Roles = (from userRole in _context.UserRoles
                                           join role in _context.Roles on userRole.RoleId equals role.Id
                                           where userRole.UserId == u.Id
                                           select role).ToList() // Fetch the user's roles
                              }).FirstOrDefaultAsync();

            return user;
        }

        /// <summary>
        /// Get users. (admin use)
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        public async Task<PaginationModel<UserViewModel>> GetUsersAsync(int currentPage, int itemsPerPage, string keyword = null, string sort = null)
        {
            // Start with the base query for users
            var query = from user in _context.Users
                        select new
                        {
                            user.Id,
                            user.UserName,
                            user.Email,
                            Roles = (from userRole in _context.UserRoles
                                     join role in _context.Roles on userRole.RoleId equals role.Id
                                     where userRole.UserId == user.Id
                                     select role).ToList() // Fetch IdentityRole for each user
                        };

            // Filter by keyword if provided (search by username or email)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.UserName.Contains(keyword) || u.Email.Contains(keyword));
            }

            // Apply sorting if provided
            if (!string.IsNullOrEmpty(sort))
            {
                query = sort switch
                {
                    "username_asc" => query.OrderBy(u => u.UserName),
                    "username_desc" => query.OrderByDescending(u => u.UserName),
                    _ => query.OrderBy(u => u.UserName) // Default sorting by username
                };
            }

            // Get total items before pagination
            var totalItems = await query.CountAsync();

            // Calculate total pages based on the number of items per page
            var totalPages = (int)System.Math.Ceiling(totalItems / (double)itemsPerPage);

            // Apply pagination and fetch users with roles
            var users = await query
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            // Map the result into UserViewModel
            var userViewModels = users.Select(u => new UserViewModel
            {
                User = new UserModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email
                },
                Roles = u.Roles // List of IdentityRole for the user
            }).ToList();

            // Return paginated result with user and role information
            return new PaginationModel<UserViewModel>(userViewModels, currentPage, itemsPerPage, totalItems, totalPages)
            {
                Keyword = keyword,
                Sort = sort
            };
        }

        #region Organization

        /// <summary>
        /// Get organization by id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public async Task<OrganizationModel> GetOrganizationByIdAsync(string organizationId)
        {
            return await _context.Organizations.FindAsync(organizationId);
        }

        /// <summary>
        /// Get organization view by id
        /// </summary>
        /// <returns></returns>
        public async Task<OrganizationViewModel> GetOrganizationViewByIdAsync(string organizationId)
        {
            var organizationView = await (from o in _context.Organizations
                                          where o.OrganizationId == organizationId
                                          select new OrganizationViewModel
                                          {
                                              Organization = o,
                                              Members = (from m in _context.Memberships
                                                         where m.OrganizationId == o.OrganizationId
                                                         select m).ToList()
                                          }).FirstOrDefaultAsync();
            return organizationView;
        }

        /// <summary>
        /// Retrieves a list of organizations the user is a member of, with optional filtering and sorting.
        /// </summary>
        /// <param name="userId">The ID of the user whose organizations are being fetched.</param>
        /// <param name="keyword">An optional keyword for filtering organizations by name or description.</param>
        /// <param name="sort">An optional sort parameter to define the sorting order.</param>
        /// <returns>A list of organizations with their details and members.</returns>
        public async Task<List<OrganizationViewModel>> GetMyOrganizationsAsync(string userId,
            string? keyword, string? sort)
        {
            // Validate userId.
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("UserId is required", nameof(userId));
            }

            var myOrganizations = from m in _context.Organizations
                                  join o in _context.Memberships on m.OrganizationId equals o.OrganizationId
                                  where o.UserId == userId
                                  select new OrganizationViewModel
                                  {
                                      Organization = m,
                                      Members = (from mm in _context.Memberships
                                                 where mm.OrganizationId == m.OrganizationId
                                                 select mm).ToList()
                                  };

            // Filter and sort.
            if (!string.IsNullOrEmpty(keyword))
            {
                myOrganizations = myOrganizations
                    .Where(o => o.Organization.OrganizationName.ToLower().Contains(keyword.ToLower()) ||
                                o.Organization.OrganizationDescription.ToLower().Contains(keyword.ToLower()));
            }

            // Apply sorting based on the provided sort parameter.
            myOrganizations = sort switch
            {
                "name_asc" => myOrganizations.OrderBy(o => o.Organization.OrganizationName),
                "name_desc" => myOrganizations.OrderByDescending(o => o.Organization.OrganizationName),
                "created_asc" => myOrganizations.OrderBy(o => o.Organization.Created),
                "created_desc" => myOrganizations.OrderByDescending(o => o.Organization.Created),
                _ => myOrganizations.OrderByDescending(o => o.Organization.Created) // Default sorting: by creation date descending.
            };

            // Execute the query and return the results as a list.
            return await myOrganizations.ToListAsync();
        }

        /// <summary>
        /// Upsert organization
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        public async Task<OrganizationModel> UpsertOrganizationAsync(OrganizationModel organization)
        {
            var original = await GetOrganizationByIdAsync(organization.OrganizationId);

            if (original == null)
            {
                organization.Created = DateTimeOffset.UtcNow;
                organization.Updated = DateTimeOffset.UtcNow;

                // New
                _context.Organizations.Add(organization);

                // Add the user as the owner
                var user = _context.Users.Find(organization.CreatedBy);

                if (user != null)
                {
                    var member = new OrganizationMemberModel
                    {
                        MemberId = Guid.NewGuid().ToString(),
                        OrganizationId = organization.OrganizationId,
                        UserId = user.Id,
                        Role = "Owner",
                        Status = "Active",
                        Created = DateTimeOffset.UtcNow,
                        Updated = DateTimeOffset.UtcNow,
                        Joined = DateTimeOffset.UtcNow
                    };

                    _context.Memberships.Add(member);

                    if (string.IsNullOrEmpty(user.OrganizationId))
                    {
                        user.OrganizationId = organization.OrganizationId;

                        _context.Users.Update(user);
                    }
                }

                await _context.SaveChangesAsync();

                return organization;
            }

            original.OrganizationName = organization.OrganizationName;
            original.OrganizationType = organization.OrganizationType;
            original.OrganizationDescription = organization.OrganizationDescription;
            original.OrganizationLogo = organization.OrganizationLogo;
            original.OrganizationWebsite = organization.OrganizationWebsite;
            original.OrganizationEmail = organization.OrganizationEmail;
            original.OrganizationPhone = organization.OrganizationPhone;
            original.OrganizationAddress = organization.OrganizationAddress;
            original.OrganizationCity = organization.OrganizationCity;
            original.OrganizationState = organization.OrganizationState;
            original.OrganizationCountry = organization.OrganizationCountry;
            original.Updated = DateTimeOffset.UtcNow;

            // Upsert
            _context.Organizations.Update(original);
            await _context.SaveChangesAsync();

            return original;
        }

        /// <summary>
        /// Upsert membership
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public async Task<OrganizationMemberModel> UpsertOrganizationMemberAsync(OrganizationMemberModel member)
        {
            var original = await (from m in _context.Memberships
                                  where m.MemberId == member.MemberId
                                  select m).FirstOrDefaultAsync();

            if (original == null)
            {
                // New
                _context.Memberships.Add(member);
                await _context.SaveChangesAsync();

                return member;
            }

            // Upsert
            _context.Memberships.Update(member);
            await _context.SaveChangesAsync();

            return original;
        }

        #endregion

        #region login link

        /// <summary>
        /// Issue login link
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<LoginLinkModel> IssueLoginLinkAsync(string userId)
        {
            var loginLink = new LoginLinkModel
            {
                UserId = userId,
                LinkId = Guid.NewGuid().ToString(),
                Expire = DateTimeOffset.UtcNow.AddHours(1)
            };

            _context.LoginLinks.Add(loginLink);

            await _context.SaveChangesAsync();

            return loginLink;
        }

        /// <summary>
        /// Consume login link
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="linkId"></param>
        /// <returns></returns>
        public async Task<bool> ConsumeLoginLinkAsync(string userId, string linkId)
        {
            var original = await (from l in _context.LoginLinks
                                  where l.UserId == userId && l.LinkId == linkId
                                  select l).FirstOrDefaultAsync();

            if(original == null)
            {
                return false;
            }

            _context.LoginLinks.Remove(original);

            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Clean expired login link
        /// </summary>
        /// <returns></returns>
        public async Task CleanExpiredLoginLinkAsync()
        {
            var expiredLinks = from l in _context.LoginLinks
                               where l.Expire < DateTimeOffset.UtcNow
                               select l;

            _context.LoginLinks.RemoveRange(expiredLinks);

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}