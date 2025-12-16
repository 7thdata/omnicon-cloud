using clsCms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace clsCms.Interfaces
{
    public interface IFileSharingServices
    {
     
        Task DeleteFileAsync(string id);
    }
}
