using clsCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface ICategoryServices
    {
        /// <summary>
        /// Get channels.
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="keyword"></param>
        /// <param name="sort"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        PaginationModel<ChannelModel> GetChannels(string ownerId, string keyword, string sort, int currentPage, int itemsPerPage);

        /// <summary>
        /// Get channel by perma name
        /// </summary>
        /// <param name="ownerId"></param>
        /// <param name="permaName"></param>
        /// <returns></returns>
        ChannelModel? GetChannelByPermaName(string ownerId, string permaName);

        /// <summary>
        /// Upsert channel
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<ChannelModel?> UpsertChannelAsync(ChannelModel item);
    }

}
