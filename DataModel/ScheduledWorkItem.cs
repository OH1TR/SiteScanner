using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class ScheduledWorkItem
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        DateTime LastRun { get; set; }
        int Interval { get; set; }
        WorkItem Work { get; set; }

    }
}
