using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Models
{
    /// <summary>
    /// Represents metadata for a file in Azure Blob Storage.
    /// </summary>
    public class FileMetadataModel
    {
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime? LastModified { get; set; }
        public string ETag { get; set; }
        public string Url { get; set; }
    }

    [Table("FileShares")]
    public class FilesSharingFileModel
    {
        [Key]
        [MaxLength(64)]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");   // 32 chars no dashes

        [MaxLength(64)]
        public string ChannelId { get; set; }    // BoxId

        [MaxLength(256)]
        public string Location { get; set; }     // Path like /uploads/boxid/filename.jpg

        public int FileSize { get; set; }         // In bytes

        [MaxLength(64)]
        public string FileOwner { get; set; }     // Name of uploader

        [MaxLength(256)]
        public string Email { get; set; }         // Email (optional)

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Uploaded timestamp
        public string Thumbnail { get; set; } // Thumbnail URL (optional)
    }

}
