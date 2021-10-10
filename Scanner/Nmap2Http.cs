using Newtonsoft.Json;
using Scanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Scanner
{
    class Nmap2Http
    {
        public void Process(string workPath)
        {
            if (!System.IO.File.Exists(workPath + "\\scan.xml"))
                return;

            WorkItem originalWorkItem= JsonConvert.DeserializeObject<WorkItem>(System.IO.File.ReadAllText(workPath+ "\\workitem.json"));

            XmlDocument doc = new XmlDocument();
            doc.Load(workPath + "\\scan.xml");
            XmlNodeList hosts = doc.SelectNodes("/nmaprun/host");
            foreach (XmlNode host in hosts)
            {
                var ports = host.SelectNodes("ports/port");
                foreach (XmlNode port in ports)
                {
                    string protocol = port.Attributes["protocol"].InnerText;
                    int portid = int.Parse(port.Attributes["portid"].InnerText);
                    string state = port.SelectSingleNode("state").Attributes["state"].InnerText;

                    if(protocol=="tcp" && portid == 80 && state=="open")
                    {
                        WorkQueue.Instance.Push(new WorkItem() { Id = Guid.NewGuid(),Host= originalWorkItem.Host, Command = "Selenium", Parameters = new[] { "http://"+originalWorkItem.Parameters[0] } });
                    }

                    if (protocol == "tcp" && portid == 443 && state == "open")
                    {
                        WorkQueue.Instance.Push(new WorkItem() { Id = Guid.NewGuid(), Host = originalWorkItem.Host, Command = "Selenium", Parameters = new[] { "https://" + originalWorkItem.Parameters[0] } });
                    }

                    if (protocol == "tcp" && portid >=8080 && portid <= 8089 && state == "open")
                    {
                        WorkQueue.Instance.Push(new WorkItem() { Id = Guid.NewGuid(), Host = originalWorkItem.Host, Command = "Selenium", Parameters = new[] { "http://" + originalWorkItem.Parameters[0]+":"+portid } });
                    }
                }
            }

        }
    }
}
