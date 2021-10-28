using DataModel;
using DataModel.ModuleInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ScannerBot.Modules
{
    class DomainFinderPA : IModule
    {
        private ScannerModel _scannerModel;

        public bool IPAware => false;

        public DomainFinderPA(ScannerModel scannerModel)
        {
            _scannerModel = scannerModel;
        }

        public IPAddress GetIP(WorkItem item)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
        }

        public void ProcessItem(WorkItem item)
        {
            var data = File.ReadAllText(Path.Combine(Scope.WorkDir, "result.json"));
            ReverseWhoisResult myDeserializedClass = JsonConvert.DeserializeObject<ReverseWhoisResult>(data);
            foreach(var domain in myDeserializedClass.domainsList)
            {
                _scannerModel.PushWorkItem(new WorkItem() { Command = "Nmap", Parameters = new[] { domain }, Created = DateTime.UtcNow, Id = Guid.NewGuid(), Host = domain,PostEvents=new[] { "NmapPA"} });
            }
        }

        public void Shutdown()
        {
        }
    }
}
