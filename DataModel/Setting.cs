using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    public class Setting
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public string Module { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
