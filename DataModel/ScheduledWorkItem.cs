using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class ScheduledWorkItem
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastScheduledTime { get; set; }
        public int Interval { get; set; }
        public WorkItem Work { get; set; }

    }
}
