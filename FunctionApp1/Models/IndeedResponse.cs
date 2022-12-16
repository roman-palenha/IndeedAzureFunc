using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionApp1.Models
{
    public class IndeedResponse
    {
        public int count { get; set; }
        public List<IndeedHit> hits { get; set; }
        public string indeed_final_url { get; set; }
        public object suggest_locality { get; set; }
    }
}
