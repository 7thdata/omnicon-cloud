using clsCms.Models;
using wppCms.Models;

namespace wppCms.Areas.Usr.Models
{
    public class UsrStorageIndexViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }

        public List<FileMetadataModel> Files { get; set; }
        public string? ContinuationToken { get; set; }
        public int PageSzie { get; set; }
        public string? NameFilter { get; set; }
        public string? TypeFilter { get; set; }
        public long? MinSize { get; set; }
        public long? MaxSize { get; set; }
    }

    public class UsrStorageDetailsViewModel : PageViewModel
    {
        public ChannelViewModel Channel { get; set; }

        public FileMetadataModel File { get; set; }

        public bool IsImage => File?.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? false;

        public string ThumbnailUrl { get; set; } // Optional, for images with thumbnails
    }
}
