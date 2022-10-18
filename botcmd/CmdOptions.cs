using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace botcmd
{
        public class CmdOptions
    {
        [Option('a', "add", Required = true)]
        public string Command { get; set; }

        [Option('t', "target", Required = true)]
        public string Target { get; set; }
    }
}
