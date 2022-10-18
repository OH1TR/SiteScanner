using System;
using System.Collections.Generic;
using System.Text;

namespace ScannerBot.Modules.AmassData
{
    public class Address
    {
        public string ip { get; set; }
        public string cidr { get; set; }
        public int asn { get; set; }
        public string desc { get; set; }
    }

    public class AmassResult
    {
        public string name { get; set; }
        public string domain { get; set; }
        public List<Address> addresses { get; set; }
        public string tag { get; set; }
        public List<string> sources { get; set; }
    }
}
