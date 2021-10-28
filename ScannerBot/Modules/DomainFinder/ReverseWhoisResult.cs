using System;
using System.Collections.Generic;
using System.Text;

namespace ScannerBot.Modules
{
    class ReverseWhoisResult
    {
        public object nextPageSearchAfter { get; set; }
        public int domainsCount { get; set; }
        public List<string> domainsList { get; set; }
    }
}
