using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Models
{
    public class AppConfigModel
    {
        public GaConfigModel Ga { get; set; }
        public AzureStorageConfigModel AzureStorage { get; set; }
        public SendGridConfigModel SendGrid { get; set; }
    }

    public class GaConfigModel
    {
        public string TagId { get; set; }
    }
    public class AzureStorageConfigModel{

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

}
