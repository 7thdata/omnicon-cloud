using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface ITickServices
    {
        /// <summary>
        /// Aggregate Hourly Impression Data.
        /// </summary>
        /// <returns></returns>
        Task AggregateHourlyImpressionsAsync();

        /// <summary>
        /// Aggregate Daily Impression Data from Hourly Data and Clean Up Old Hourly Data.
        /// </summary>
        /// <returns></returns>
        Task AggregateDailyImpressionsAsync();

        /// <summary>
        /// Aggregate Monthly Impression Data from Daily Data and Clean Up Old Daily Data.
        /// </summary>
        /// <returns></returns>
        Task AggregateMonthlyImpressionsAsync();
    }
}
