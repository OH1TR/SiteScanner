using System;
using System.Collections.Generic;
using System.Text;

namespace Archiver
{

    public class Regex
    {
        public string pattern { get; set; }
        public string flags { get; set; }
    }

    public class JSToken
    {
        public string type { get; set; }
        public string value { get; set; }
        public Regex regex { get; set; }
    }
}
