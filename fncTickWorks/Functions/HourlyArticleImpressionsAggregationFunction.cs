using System;
using clsCms.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fncTickWorks.Functions
{
    /// <summary>
    /// Azure Function to aggregate hourly article impressions.
    /// Triggered by a TimerTrigger at the top of every hour.
    /// </summary>
    public class HourlyArticleImpressionsAggregationFunction
    {
        private readonly ILogger _logger;
        private readonly ITickServices _tickerServices;

        /// <summary>
        /// Constructor to initialize dependencies.
        /// </summary>
        /// <param name="loggerFactory">Factory for creating a logger instance.</param>
        /// <param name="tickerServices">Service for aggregation operations.</param>
        public HourlyArticleImpressionsAggregationFunction(ILoggerFactory loggerFactory, ITickServices tickerServices)
        {
            _tickerServices = tickerServices;
            _logger = loggerFactory.CreateLogger<HourlyArticleImpressionsAggregationFunction>();
        }

        /// <summary>
        /// Function that triggers hourly to aggregate article impressions.
        /// </summary>
        /// <param name="myTimer">TimerTrigger binding providing trigger metadata.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        [Function("HourlyArticleImpressionsAggregationFunction")]
        public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, bool isDebug = false)
        {
            // Log the time of function execution for debugging purposes.
            _logger.LogInformation($"C# Timer trigger (HourlyArticleImpressionsAggregationFunction) function executed at: {DateTime.Now}");

            try
            {
                // Call the service to perform hourly aggregation.
                await _tickerServices.AggregateHourlyImpressionsAsync();
                _logger.LogInformation("Hourly aggregation completed successfully.");
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur during execution.
                _logger.LogError(ex, "An error occurred while performing hourly aggregation.");
            }
        }
    }
}
