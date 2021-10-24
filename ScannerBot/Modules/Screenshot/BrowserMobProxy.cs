using AutomatedTester.BrowserMob;
using AutomatedTester.BrowserMob.HAR;
using DataModel.ModuleInterface;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ScannerBot.Modules.Screenshot
{
    class BrowserMobProxy
    {
        public Server server;
        public Client client;

        public BrowserMobProxy(IConfig config)
        {
            server = new Server(config.GetSetting(typeof(BrowserMobProxy),"Path"));
            server.Start();

            System.Threading.Thread.Sleep(3000);

            client = server.CreateProxy();

            System.Threading.Thread.Sleep(10000);

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
