using wppCms.Models;

namespace wppCms.Areas.Admin.Models
{

    public class AdminSearchIndexViewModel : PageViewModel
    {
        public List<string> Indexes { get; set; }
    }

    public class AdminSearchIndexersViewModel : PageViewModel
    {
        public List<string> Indexers { get; set; }
    }

    public class AdminSearchDataSourcesViewModel : PageViewModel
    {
        public List<string> DataSources { get; set; } = new List<string>();
    }


    public class AdminSearchIndexStatisticsViewModel : PageViewModel
    {
        public string IndexName { get; set; }
        public long DocumentCount { get; set; }
        public double StorageSize { get; set; } // In MB
    }
}
