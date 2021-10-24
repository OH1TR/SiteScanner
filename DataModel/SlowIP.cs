using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class SlowIP
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string IPAddress { get; set; }
    }
}
