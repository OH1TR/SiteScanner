using Microsoft.Extensions.Configuration;
using System;

namespace Archiver
{
    class Program
    {
        public static IConfiguration Configuration;

        static void Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", true, true)
               .Build();

           
            Downloader dl = new Downloader();
            dl.Process();
           /*
            var aw = new ArchiveDBWorker();
            aw.ProcessFolder();
           */

        }
    }
}
