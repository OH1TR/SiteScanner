using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ScannerWebSite.Model
{
    public class VMControlRequest
    {
        [Required]
        public string ApiKey { get; set; }

        [Required]
        public string Command { get; set; }
    }
}
