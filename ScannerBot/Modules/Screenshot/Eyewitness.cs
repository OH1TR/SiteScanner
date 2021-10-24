using DataModel;
using DataModel.ModuleInterface;
using ScannerBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ScannerBot.Modules.Screenshot
{

    class Eyewitness : IModule
    {
        public BrowserMobProxy _browserMob;

        IConfig _config;
        private ShellCommandRunner _commandRunner;

        public Eyewitness(IConfig config, ShellCommandRunner commandRunner)
        {
            _config = config;
            _commandRunner = commandRunner;
        }

        public bool IPAware => false;

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
            _browserMob = new BrowserMobProxy(_config);
 
        }

        public void ProcessItem(WorkItem item)
        {
            /*
            string proxy = mb.GetProxy();
            "EyeWitness.py --web --no-prompt -d #TMP# --single http://#HOST#"



                            Console.WriteLine("Url:" + item.Parameters[0]);
            string workDir = Path.Combine(ConfigurationManager.AppSettings["WorkDir"], item.Id.ToString());
            Directory.CreateDirectory(workDir);
            mb.NewHar(item.Id.ToString().Replace("-", ""));
            wd.OpenURL(item.Parameters[0]);
            wd.Screenshot(Path.Combine(workDir, "image.jpg"));
            mb.SaveResultToFile(Path.Combine(workDir, "traffic.har"));
            File.WriteAllText(Path.Combine(workDir, "workitem.json"), JsonConvert.SerializeObject(item));


            string resultPath = Path.Combine(Scope.WorkDir, "scan.xml");
            Console.WriteLine(_commandRunner.RunCommand(_config.GetSetting(typeof(Nmap), "Path") + " -p 80,443,8080-8089 -Pn --host-timeout 30s -oX " + resultPath + " " + item.Parameters[0]));
            scans.Add(ip, item.Id.ToString());
            */

            _browserMob.NewHar(item.Id.ToString().Replace("-", ""));
            string[] proxy = _browserMob.GetProxy().Split(':');
            Console.WriteLine(_browserMob.GetProxy());
            string resultPath = Path.Combine(Scope.WorkDir, "scan");
            System.Threading.Thread.Sleep(3000);
            Console.WriteLine("*** eye start");
            Console.WriteLine(_commandRunner.RunCommand(_config.GetSetting(typeof(Eyewitness), "Path") + " --selenium-log-path "+ resultPath + ".log --web --no-prompt --proxy-ip 127.0.0.1 --proxy-port " + proxy[1] + " -d " + resultPath + " --single " + item.Parameters[0]));
            Console.WriteLine("*** eye end");
            _browserMob.SaveResultToFile(Path.Combine(Scope.WorkDir, "traffic.har"));
            Console.WriteLine("*** harvest end");

        }

        public void Shutdown()
        {
            _browserMob.server.Stop();
        }
    }
}

