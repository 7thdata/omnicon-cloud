using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Models
{
    public class PaginationModel<T>
    {
        public PaginationModel(IList<T> items, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            Items = items;
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }

        public IList<T>? Items { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public string? Keyword { get; set; }
        public string? Sort { get; set; }
        public string? ContiuationToken { get; set; }
    }
}
