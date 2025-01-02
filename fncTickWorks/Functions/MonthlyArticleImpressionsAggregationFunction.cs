using System;
using clsCms.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fncTickWorks.Functions
{
    /// <summary>
    /// Azure Function to aggregate monthly article impressions.
    /// Triggered by a TimerTrigger at midnight UTC on the first day of every month.
    /// </summary>
    public class MonthlyArticleImpressionsAggregationFunction
    {
        private readonly ILogger _logger; // Logger for logging function execution details.
        private readonly ITickServices _tickServices; // Service to handle aggregation logic.

        /// <summary>
        /// Constructor to initialize dependencies.
        /// </summary>
        /// <param name="loggerFactory">Factory for creating a logger instance.</param>
        /// <param name="tickServices">Service for aggregation operations.</param>
        public MonthlyArticleImpressionsAggregationFunction(ILoggerFactory loggerFactory, ITickServices tickServices)
        {
            _tickServices = tickServices ?? throw new ArgumentNullException(nameof(tickServices)); // Ensure dependency is provided.
            _logger = loggerFactory?.CreateLogger<MonthlyArticleImpressionsAggregationFunction>()
                ?? throw new ArgumentNullException(nameof(loggerFactory)); // Ensure loggerFactory is provided.
        }

        /// <summary>
        /// Function that triggers monthly to aggregate article impressions.
        /// </summary>
        /// <param name="myTimer">TimerTrigger binding providing trigger metadata.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [Function("MonthlyArticleImpressionsAggregationFunction")]
        public async Task Run([TimerTrigger("0 0 0 1 * *")] TimerInfo myTimer)
        {
            // Log the time of function execution for debugging purposes.
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");

            try
            {
                // Call the service to perform monthly aggregation.
                await _tickServices.AggregateMonthlyImpressionsAsync();
                _logger.LogInformation("Monthly aggregation completed successfully.");
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during execution.
                _logger.LogError(ex, "An error occurred while performing monthly aggregation.");
            }
        }
    }
}
