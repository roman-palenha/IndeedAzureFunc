using FunctionApp1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunctionApp1.Services.Interfaces
{
    public interface ISearchJobService
    {
       Task<IEnumerable<IndeedHit>> SendRequestAsync(string uri, ApiConfiguration apiConfiguration);
    }
}
