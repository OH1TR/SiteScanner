using AutomatedTester.BrowserMob;
using AutomatedTester.BrowserMob.HAR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Scanner
{
    class MobProxy
    {
        public Server server;
        public Client client;

        public MobProxy()
        {
            server = new Server(ConfigurationManager.AppSettings["BrowserMobPath"]);
            server.Start();
            
            client = server.CreateProxy();
            
        }

        public string GetProxy()
        {
            return client.SeleniumProxy;
        }

        public void NewHar(string name)
        {
            client.NewHar(name,true);
        }

        public HarResult GetResult()
        {
            return client.GetHar();
        }

        public void SaveResultToFile(string fileName)
        {
            var data=client.GetHarRaw();
            System.IO.File.WriteAllBytes(fileName, data);
        }

        public void Terminate()
        {
            client.Close();
            server.Stop();
        }
    }
}
