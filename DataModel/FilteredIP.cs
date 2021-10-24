using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class FilteredIP
    {
        public Guid Id { get; set; }
        public string IPAddress { get; set; }
        public string Reason { get; set; }
    }
}
