using clsCMs.Data;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.EntityFrameworkCore;

namespace clsCms.Services
{
    public class FileSharingServices : IFileSharingServices
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobStorageServices _blobStorageServices;

        public FileSharingServices(ApplicationDbContext db, IBlobStorageServices blobStorageServices)
        {
            _db = db;
            _blobStorageServices = blobStorageServices;
        }


        public async Task<FilesSharingFileModel> GetFileAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return await _db.FileSharingFiles.FindAsync(id);
        }

        /// <summary>
        /// Get files by box id.
        /// </summary>
        /// <param name="boxId"></param>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        public async Task<PaginationModel<FilesSharingFileModel>> GetFilesByBoxIdAsync(
    string boxId,
    string? search,
    string? sort,
    int currentPage,
    int itemsPerPage)
        {
            if (string.IsNullOrWhiteSpace(boxId))
            {
                return new PaginationModel<FilesSharingFileModel>(new List<FilesSharingFileModel>(), currentPage, itemsPerPage, 0, 0);
            }

            var query = _db.FileSharingFiles.AsQueryable();

            query = query.Where(x => x.ChannelId == boxId);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>
                    x.FileOwner.Contains(search) ||
                    x.Email.Contains(search) ||
                    x.Location.Contains(search));
            }

            // Sorting
            query = sort?.ToLower() switch
            {
                "name" => query.OrderBy(x => x.FileOwner),
                "name_desc" => query.OrderByDescending(x => x.FileOwner),
                "date" => query.OrderBy(x => x.UploadedAt),
                "date_desc" => query.OrderByDescending(x => x.UploadedAt),
                _ => query.OrderByDescending(x => x.UploadedAt)
            };

            // Count total
            var totalItems = await query.CountAsync();

            // Calculate pages
            var totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            // Fetch current page
            var items = await query
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            return new PaginationModel<FilesSharingFileModel>(
                items,
                currentPage,
                itemsPerPage,
                totalItems,
                totalPages)
            {
                Keyword = search,
                Sort = sort
            };
        }

        public async Task DeleteFileAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return;

            var file = await _db.FileSharingFiles.FindAsync(id);
            if (file != null)
            {
                _db.FileSharingFiles.Remove(file);
                await _db.SaveChangesAsync();
            }
        }

    }
}
