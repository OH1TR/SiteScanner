using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scanner.Model
{
    class WorkItem
    {
        public Guid Id { get; set; }
        public string Command { get; set; }
        public string Host { get; set; }
        public string[] Parameters { get; set; }
        public string[] PostEvents { get; set; }
    }
}
