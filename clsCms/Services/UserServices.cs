using clsCms.Models;
using clsCMs.Data;
using Microsoft.EntityFrameworkCore;

namespace clsCms.Services
{
    /// <summary>
    /// Provides user- and organization-related operations.
    ///
    /// This service acts as a boundary between:
    /// - ASP.NET Identity data (Users, Roles)
    /// - Domain-level concepts (Organizations, Memberships)
    ///
    /// Intended for both admin and authenticated user scenarios.
    /// </summary>
    public class UserServices : IUserServices
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes the <see cref="UserServices"/>.
        /// </summary>
        public UserServices(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Users (Admin)

        /// <summary>
        /// Retrieves a single user by ID, including assigned roles.
        /// Intended for administrative use.
        /// </summary>
        public async Task<UserViewModel?> GetUserByIdAsync(string userId)
        {
            return await (
                from u in _context.Users
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
                    Roles =
                        (from ur in _context.UserRoles
                         join r in _context.Roles on ur.RoleId equals r.Id
                         where ur.UserId == u.Id
                         select r).ToList()
                }
            ).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a paginated list of users with role information.
        /// Intended for administrative dashboards.
        /// </summary>
        public async Task<PaginationModel<UserViewModel>> GetUsersAsync(
            int currentPage,
            int itemsPerPage,
            string? keyword = null,
            string? sort = null,
            bool fetchOnlyEmailConfirmed = true)
        {
            // -----------------------------------------
            // Base query
            // -----------------------------------------
            var query =
                from u in _context.Users
                select new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.EmailConfirmed,
                    Roles =
                        (from ur in _context.UserRoles
                         join r in _context.Roles on ur.RoleId equals r.Id
                         where ur.UserId == u.Id
                         select r).ToList()
                };

            // -----------------------------------------
            // Filtering
            // -----------------------------------------
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(u =>
                    u.UserName.Contains(keyword) ||
                    u.Email.Contains(keyword));
            }

            if (fetchOnlyEmailConfirmed)
            {
                query = query.Where(u => u.EmailConfirmed);
            }

            // -----------------------------------------
            // Sorting
            // -----------------------------------------
            query = sort switch
            {
                "username_desc" => query.OrderByDescending(u => u.UserName),
                _ => query.OrderBy(u => u.UserName)
            };

            // -----------------------------------------
            // Pagination
            // -----------------------------------------
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var users = await query
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            var result = users.Select(u => new UserViewModel
            {
                User = new UserModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    EmailConfirmed = u.EmailConfirmed
                },
                Roles = u.Roles
            }).ToList();

            return new PaginationModel<UserViewModel>(
                result,
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

        #region Organizations

        /// <summary>
        /// Retrieves an organization by its identifier.
        /// </summary>
        public async Task<OrganizationModel?> GetOrganizationByIdAsync(string organizationId)
        {
            return await _context.Organizations.FindAsync(organizationId);
        }

        /// <summary>
        /// Retrieves a paginated list of organizations.
        /// Optionally filtered by user membership.
        /// </summary>
        public PaginationModel<OrganizationModel> GetOrganizations(
            string keyword,
            string sort,
            int currentPage,
            int itemsPerPage,
            string? userId)
        {
            IQueryable<OrganizationModel> query =
                _context.Organizations.AsNoTracking();

            // -----------------------------------------
            // Filter by membership
            // -----------------------------------------
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o =>
                    _context.Memberships.Any(m =>
                        m.OrganizationId == o.OrganizationId &&
                        m.UserId == userId));
            }

            // -----------------------------------------
            // Keyword search
            // -----------------------------------------
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(o =>
                    o.OrganizationName!.Contains(keyword));
            }

            // -----------------------------------------
            // Sorting
            // -----------------------------------------
            query = sort switch
            {
                "name-desc" => query.OrderByDescending(o => o.OrganizationName),
                "created-asc" => query.OrderBy(o => o.Created),
                "created-desc" => query.OrderByDescending(o => o.Created),
                _ => query.OrderBy(o => o.OrganizationName)
            };

            // -----------------------------------------
            // Pagination
            // -----------------------------------------
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var items = query
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            return new PaginationModel<OrganizationModel>(
                items,
                currentPage,
                itemsPerPage,
                totalItems,
                totalPages)
            {
                Keyword = keyword,
                Sort = sort
            };
        }

        /// <summary>
        /// Retrieves an organization with its members.
        /// </summary>
        public async Task<OrganizationViewModel?> GetOrganizationViewByIdAsync(string organizationId)
        {
            if (string.IsNullOrWhiteSpace(organizationId))
                throw new ArgumentException("Organization ID is required.", nameof(organizationId));

            return await _context.Organizations
                .Where(o => o.OrganizationId == organizationId)
                .Select(o => new OrganizationViewModel
                {
                    Organization = o,
                    Members =
                        _context.Memberships
                            .Where(m => m.OrganizationId == o.OrganizationId)
                            .Join(
                                _context.Users,
                                m => m.UserId,
                                u => u.Id,
                                (m, u) => new OrganizationMemberViewModel
                                {
                                    Member = m,
                                    User = u
                                })
                            .ToList()
                })
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves all organizations the user belongs to.
        /// Used for dashboards and navigation.
        /// </summary>
        public async Task<List<OrganizationViewModel>> GetMyOrganizationsAsync(
            string userId,
            string? keyword,
            string? sort)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));

            var query =
                _context.Organizations
                    .Where(o =>
                        _context.Memberships.Any(m =>
                            m.OrganizationId == o.OrganizationId &&
                            m.UserId == userId))
                    .Select(o => new OrganizationViewModel
                    {
                        Organization = o,
                        Members =
                            _context.Memberships
                                .Where(m => m.OrganizationId == o.OrganizationId)
                                .Join(
                                    _context.Users,
                                    m => m.UserId,
                                    u => u.Id,
                                    (m, u) => new OrganizationMemberViewModel
                                    {
                                        Member = m,
                                        User = u
                                    })
                                .ToList()
                    });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var k = keyword.ToLower();
                query = query.Where(o =>
                    o.Organization.OrganizationName.ToLower().Contains(k) ||
                    o.Organization.OrganizationDescription.ToLower().Contains(k));
            }

            query = sort switch
            {
                "name_asc" => query.OrderBy(o => o.Organization.OrganizationName),
                "name_desc" => query.OrderByDescending(o => o.Organization.OrganizationName),
                "created_asc" => query.OrderBy(o => o.Organization.Created),
                _ => query.OrderByDescending(o => o.Organization.Created)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Creates or updates an organization.
        /// Automatically assigns the creator as Owner on creation.
        /// </summary>
        public async Task<OrganizationModel> UpsertOrganizationAsync(OrganizationModel organization)
        {
            var existing = await GetOrganizationByIdAsync(organization.OrganizationId);

            if (existing == null)
            {
                organization.Created = DateTimeOffset.UtcNow;
                organization.Updated = DateTimeOffset.UtcNow;

                _context.Organizations.Add(organization);

                var creator = _context.Users.Find(organization.CreatedBy);
                if (creator != null)
                {
                    _context.Memberships.Add(new OrganizationMemberModel
                    {
                        MemberId = Guid.NewGuid().ToString(),
                        OrganizationId = organization.OrganizationId,
                        UserId = creator.Id,
                        Role = "Owner",
                        Status = "Active",
                        Created = DateTimeOffset.UtcNow,
                        Updated = DateTimeOffset.UtcNow,
                        Joined = DateTimeOffset.UtcNow
                    });

                    if (string.IsNullOrEmpty(creator.OrganizationId))
                    {
                        creator.OrganizationId = organization.OrganizationId;
                        _context.Users.Update(creator);
                    }
                }

                await _context.SaveChangesAsync();
                return organization;
            }

            // Update existing
            existing.OrganizationName = organization.OrganizationName;
            existing.OrganizationType = organization.OrganizationType;
            existing.OrganizationDescription = organization.OrganizationDescription;
            existing.OrganizationLogo = organization.OrganizationLogo;
            existing.OrganizationWebsite = organization.OrganizationWebsite;
            existing.OrganizationEmail = organization.OrganizationEmail;
            existing.OrganizationPhone = organization.OrganizationPhone;
            existing.OrganizationAddress = organization.OrganizationAddress;
            existing.OrganizationCity = organization.OrganizationCity;
            existing.OrganizationState = organization.OrganizationState;
            existing.OrganizationCountry = organization.OrganizationCountry;
            existing.Updated = DateTimeOffset.UtcNow;

            _context.Organizations.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        /// <summary>
        /// Creates or updates an organization membership.
        /// </summary>
        public async Task<OrganizationMemberModel> UpsertOrganizationMemberAsync(
            OrganizationMemberModel member)
        {
            var existing =
                await _context.Memberships
                    .FirstOrDefaultAsync(m => m.MemberId == member.MemberId);

            if (existing == null)
            {
                member.Created = DateTimeOffset.UtcNow;
                member.Updated = DateTimeOffset.UtcNow;

                _context.Memberships.Add(member);
                await _context.SaveChangesAsync();

                return member;
            }

            member.Updated = DateTimeOffset.UtcNow;
            _context.Memberships.Update(member);
            await _context.SaveChangesAsync();

            return existing;
        }

        #endregion

        #region Login links

        /// <summary>
        /// Issues a one-time login link for passwordless authentication.
        /// </summary>
        public async Task<LoginLinkModel> IssueLoginLinkAsync(string userId)
        {
            var link = new LoginLinkModel
            {
                UserId = userId,
                LinkId = Guid.NewGuid().ToString(),
                Expire = DateTimeOffset.UtcNow.AddHours(1)
            };

            _context.LoginLinks.Add(link);
            await _context.SaveChangesAsync();

            return link;
        }

        /// <summary>
        /// Consumes and invalidates a login link.
        /// </summary>
        public async Task<bool> ConsumeLoginLinkAsync(string userId, string linkId)
        {
            var link =
                await _context.LoginLinks
                    .FirstOrDefaultAsync(l =>
                        l.UserId == userId &&
                        l.LinkId == linkId);

            if (link == null)
                return false;

            _context.LoginLinks.Remove(link);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Removes expired login links.
        /// Intended to be run by background jobs.
        /// </summary>
        public async Task CleanExpiredLoginLinkAsync()
        {
            var expired =
                _context.LoginLinks
                    .Where(l => l.Expire < DateTimeOffset.UtcNow);

            _context.LoginLinks.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
