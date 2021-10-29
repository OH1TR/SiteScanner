using DataModel;
using DataModel.ModuleInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace ScannerBot.Modules
{
    class NmapPA : IModule
    {
        private ScannerModel _scannerModel;

        public bool IPAware => false;

        public NmapPA(ScannerModel scannerModel)
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
            if (!File.Exists(Path.Combine(Scope.WorkDir, "scan.xml")))
            {
                Console.WriteLine("No scan.xml");
                return;
            }

            WorkItem originalWorkItem = JsonConvert.DeserializeObject<WorkItem>(File.ReadAllText(Path.Combine(Scope.WorkDir, "workitem.json")));

            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(Scope.WorkDir, "scan.xml"));
            XmlNodeList hosts = doc.SelectNodes("/nmaprun/host");
            foreach (XmlNode host in hosts)
            {
                var ports = host.SelectNodes("ports/port");
                foreach (XmlNode port in ports)
                {
                    string protocol = port.Attributes["protocol"].InnerText;
                    int portid = int.Parse(port.Attributes["portid"].InnerText);
                    string state = port.SelectSingleNode("state").Attributes["state"].InnerText;

                    if (protocol == "tcp" && portid == 80 && state == "open")
                    {
                        Console.WriteLine("Adding work item");
                        _scannerModel.PushWorkItem(new WorkItem() { Id = Guid.NewGuid(), Created = DateTime.UtcNow, Host = originalWorkItem.Host, Command = "Screenshot", Parameters = new[] { "http://" + originalWorkItem.Parameters[0] } });
                    }

                    if (protocol == "tcp" && (portid == 443 || portid == 8443) && state == "open")
                    {
                        Console.WriteLine("Adding work item");
                        _scannerModel.PushWorkItem(new WorkItem() { Id = Guid.NewGuid(), Created = DateTime.UtcNow, Host = originalWorkItem.Host, Command = "Screenshot", Parameters = new[] { "https://" + originalWorkItem.Parameters[0] } });
                    }

                    if (protocol == "tcp" && portid >= 8080 && portid <= 8089 && state == "open")
                    {
                        Console.WriteLine("Adding work item");
                        _scannerModel.PushWorkItem(new WorkItem() { Id = Guid.NewGuid(), Created = DateTime.UtcNow, Host = originalWorkItem.Host, Command = "Screenshot", Parameters = new[] { "http://" + originalWorkItem.Parameters[0] + ":" + portid } });
                    }
                }
            }

        }

        public void Shutdown()
        {

        }
    }
}
