using FunctionApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Services.Interfaces
{
    public interface IGetJobDetailsService
    {
        Task<IndeedJobDetails> SendRequestAsync(string uri, ApiConfiguration apiConfiguration);
    }
}
