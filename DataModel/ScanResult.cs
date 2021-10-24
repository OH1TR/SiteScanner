using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class ScanResult
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public int Code { get; set; }
    }
}
