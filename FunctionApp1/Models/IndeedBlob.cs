using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Models
{
    public class IndeedBlob
    {
        public string Description { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public override int GetHashCode()
        {
            return Description.GetHashCode() ^ Title.GetHashCode() ^ Url.GetHashCode();
        }
    }
}
