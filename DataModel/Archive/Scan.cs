using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Archive
{
    public class Scan
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public WorkItem WorkItem { get; set; }
        public string ArchivePath { get; set; }
    }
}
