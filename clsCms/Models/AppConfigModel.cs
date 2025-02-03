using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Models
{
    public class AppConfigModel
    {
        public FontAwsomeModel FontAwsome { get; set; }
        public GaConfigModel Ga { get; set; }
        public AzureStorageConfigModel AzureStorage { get; set; }
        public SendGridConfigModel SendGrid { get; set; }
        public AzureSearchConfigModel AzureSearch { get; set; } // Added Azure Search configuration
    }

    public class FontAwsomeModel
    {
        public string KitUrl { get; set; }
    }

    public class GaConfigModel
    {
        public string TagId { get; set; }
    }

    public class AzureStorageConfigModel
    {
        public string StorageTableUri { get; set; }
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
        public string BlobConnectionString { get; set; }
        public string BlobContainerName { get; set; }
    }

    public class SendGridConfigModel
    {
        public string SendGridApiKey { get; set; }
    }

    public class AzureSearchConfigModel
    {
        public string SearchServiceName { get; set; } // Name of the Azure Cognitive Search service
        public string SearchApiKey { get; set; } // Admin API key for Azure Search
        public string IndexName { get; set; } // Default index name for articles
        public string DataSourceName { get; set; } // Data source name for the Azure Table connection
        public string IndexerName { get; set; } // Default indexer name
    }

}
