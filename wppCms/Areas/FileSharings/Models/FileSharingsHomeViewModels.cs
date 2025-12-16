using clsCms.Models;

namespace wppCms.Areas.FileSharings.Models
{
    public class FileSharingsHomeViewModels
    {
    }
    public class FileSharingsHomeIndexViewModel
    {
        public string Culture { get; set; }
    }
    public class FileSharingsHomeFilesViewModel
    {
        public string Culture { get; set; }
        public string BoxId { get; set; }
        public PaginationModel<FilesSharingFileModel> Files { get; set; }
        public UserModel CurrentUser { get; set; }
    }
}
