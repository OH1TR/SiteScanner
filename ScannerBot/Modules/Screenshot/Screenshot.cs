using DataModel;
using DataModel.ModuleInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ScannerBot.Modules.Screenshot
{
    class Screenshot : IModule
    {
        public BrowserMobProxy mb;

        WebDriver wd;
        IConfig _config;

        public Screenshot(IConfig config)
        {
            _config = config;
        }

        public bool IPAware => true;

        public IPAddress GetIP(WorkItem item)
        {
            var hostEntry = Dns.GetHostEntry(item.Parameters[0]);

            if (hostEntry.AddressList.Length > 0)
            {
                return hostEntry.AddressList[0];
            }
            return IPAddress.None;
        }

        public void Init()
        {
            mb = new BrowserMobProxy(_config);
            wd = new WebDriver(_config,mb.GetProxy());
        }

        public void ProcessItem(WorkItem item)
        {
            Console.WriteLine("Url:" + item.Parameters[0]);
            mb.NewHar(item.Id.ToString().Replace("-", ""));
            wd.OpenURL(item.Parameters[0]);
            wd.Screenshot(Path.Combine(Scope.WorkDir, "image.jpg"));
            mb.SaveResultToFile(Path.Combine(Scope.WorkDir, "traffic.har"));
        }

        public void Shutdown()
        {
            mb.Terminate();
            wd.Close();
        }
    }
}
