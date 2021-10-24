using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class WorkItem
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string Command { get; set; }
        public string Host { get; set; }
        public string[] Parameters { get; set; }
        public string[] PostEvents { get; set; }
    }
}
