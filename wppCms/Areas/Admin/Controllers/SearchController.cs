using Azure;
using Azure.Search.Documents.Indexes.Models;
using clsCms.Interfaces;
using clsCms.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using wppCms.Areas.Admin.Models;
using wppCms.Models;

namespace wppCms.Areas.Admin.Controllers
{

    [Authorize(Roles = "admin")]
    [Area("Admin")]
    [Route("/{culture}/admin/searches")]
    public class SearchController : Controller
    {
        private readonly ISearchServices _searchServices;
        private readonly AppConfigModel _appConfig;

        public SearchController(ISearchServices searchServices, IOptions<AppConfigModel> appConfig)
        {
            _searchServices = searchServices;
            _appConfig = appConfig.Value ;
        }

        private T PopulatePageViewModel<T>(string culture) where T : PageViewModel, new()
        {
            return new T
            {
                Culture = culture,
                GaTagId = _appConfig.Ga?.TagId,
                FontAwsomeUrl = _appConfig.FontAwsome?.KitUrl
            };
        }

        // Index management
        [HttpGet("")]
        public async Task<IActionResult> Index(string culture)
        {
            var indexes = await _searchServices.ListIndexesAsync();
            var viewModel = PopulatePageViewModel<AdminSearchIndexViewModel>(culture);
            viewModel.Indexes = indexes;
            return View(viewModel);
        }

        // Similarly, update other methods to use PopulatePageViewModel<T>...

        [HttpGet("indexers")]
        public async Task<IActionResult> ListIndexers(string culture)
        {
            var indexers = await _searchServices.ListIndexersAsync();
            var viewModel = PopulatePageViewModel<AdminSearchIndexersViewModel>(culture);
            viewModel.Indexers = indexers;
            return View(viewModel);
        }

        [HttpGet("data-sources")]
        public async Task<IActionResult> ListDataSources(string culture)
        {
            var dataSources = await _searchServices.ListDataSourcesAsync();
            var viewModel = PopulatePageViewModel<AdminSearchDataSourcesViewModel>(culture);
            viewModel.DataSources = dataSources;
            return View(viewModel);
        }

        [HttpGet("statistics/{indexName}")]
        public async Task<IActionResult> IndexStatistics(string culture, string indexName)
        {
            var stats = await _searchServices.GetIndexStatisticsAsync(indexName);

            var viewModel = new AdminSearchIndexStatisticsViewModel
            {
                Culture = culture,
                IndexName = indexName,
                DocumentCount = stats.DocumentCount,
                StorageSize = stats.StorageSize / (1024.0 * 1024.0) // Convert bytes to MB
            };
            return View(viewModel);
        }


        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            await _searchServices.CreateIndexAsync(indexName);
            return RedirectToAction("Index");
        }

        [HttpPost("delete-index")]
        public async Task<IActionResult> DeleteIndex(string indexName)
        {
            await _searchServices.DeleteIndexAsync(indexName);
            return RedirectToAction("Index");
        }

        [HttpPost("create-indexer")]
        public async Task<IActionResult> CreateIndexer(string culture, string dataSourceName, string indexName)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(dataSourceName) || string.IsNullOrWhiteSpace(indexName))
            {
                ModelState.AddModelError(string.Empty, "Both Data Source Name and Index Name are required.");
                var viewModel = PopulatePageViewModel<AdminSearchIndexersViewModel>(culture);
                viewModel.Indexers = await _searchServices.ListIndexersAsync();
                return View("Indexers", viewModel); // Re-display form with validation errors
            }

            try
            {
                // Call service to create the indexer
                await _searchServices.CreateIndexerAsync(dataSourceName, indexName);
                TempData["SuccessMessage"] = $"Indexer '{dataSourceName}-indexer' created successfully.";
            }
            catch (Exception ex)
            {
                // Log and display error message
                TempData["ErrorMessage"] = $"Error creating indexer: {ex.Message}";
            }

            // Redirect to the indexers list page
            return RedirectToAction("ListIndexers", new { culture });
        }

        [HttpPost("start-indexer")]
        public async Task<IActionResult> StartIndexer(string indexerName)
        {
            await _searchServices.StartIndexerAsync(indexerName);
            return RedirectToAction("ListIndexers");
        }

        [HttpPost("reset-indexer")]
        public async Task<IActionResult> ResetIndexer(string indexerName)
        {
            await _searchServices.ResetIndexerAsync(indexerName);
            return RedirectToAction("ListIndexers");
        }

        [HttpPost("delete-indexer")]
        public async Task<IActionResult> DeleteIndexer(string indexerName)
        {
            await _searchServices.DeleteIndexerAsync(indexerName);
            return RedirectToAction("ListIndexers");
        }


        [HttpPost("create-data-source")]
        public async Task<IActionResult> CreateDataSource(string dataSourceName, string tableName, string storageConnectionString)
        {
            await _searchServices.CreateDataSourceAsync(dataSourceName, tableName, storageConnectionString);
            return RedirectToAction("ListDataSources");
        }

        [HttpPost("delete-data-source")]
        public async Task<IActionResult> DeleteDataSource(string dataSourceName)
        {
            await _searchServices.DeleteDataSourceAsync(dataSourceName);
            return RedirectToAction("ListDataSources");
        }

        [HttpPost("unindex")]
        public IActionResult DeleteSpecificDataInIndex(string key)
        {
            _searchServices.UnIndexArticlesAsync(new List<string>() { key});
            return RedirectToAction("Index");
        }

    }
}
