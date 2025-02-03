using System;
using System.Collections.Generic;
using System.Linq;
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
}
