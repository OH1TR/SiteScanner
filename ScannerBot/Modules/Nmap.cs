using DataModel;
using DataModel.ModuleInterface;
using ScannerBot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace ScannerBot.Modules
{
    class Nmap : IModule
    {
        public static Dictionary<IPAddress, string> scans = new Dictionary<IPAddress, string>();
        private ScannerModel _model;
        private IConfig _config;
        ShellCommandRunner _commandRunner;

        public bool IPAware => true;

        public Nmap(ScannerModel model,IConfig config, ShellCommandRunner commandRunner)
        {
            _model = model;
            _config = config;
            _commandRunner = commandRunner;
        }

        public void Init()
        {
     
        }

        public void Shutdown()
        {

        }

        public void ProcessItem(WorkItem item)
        {
            var hostEntry = Dns.GetHostEntry(item.Parameters[0]);

            if (hostEntry.AddressList.Length > 0)
            {
                var ip = hostEntry.AddressList[0];
                if (scans.ContainsKey(ip) && File.Exists(Path.Combine(_config.WorkDir, scans[ip] , "scan.xml")))
                {
                    byte[] d = File.ReadAllBytes(Path.Combine(_config.WorkDir, scans[ip], "scan.xml"));
                    File.WriteAllBytes(Path.Combine(Scope.WorkDir,"scan.xml"), d);
                    Console.WriteLine("Scan skipped!!");
                }
                else
                {
                    string resultPath = Path.Combine(Scope.WorkDir, "scan.xml");
                    Console.WriteLine(_commandRunner.RunCommand(_config.GetSetting(typeof(Nmap),"Path") + " -p 80,443,8443,8080-8089 -Pn --host-timeout 30s -oX " + resultPath+" " + item.Parameters[0]));
                    scans.Add(ip, item.Id.ToString());
                }
            }
        }

        public IPAddress GetIP(WorkItem item)
        {
            var hostEntry = Dns.GetHostEntry(item.Parameters[0]);

            if (hostEntry.AddressList.Length > 0)
            {
               return hostEntry.AddressList[0];
            }
            return IPAddress.None;
    }
}
}
