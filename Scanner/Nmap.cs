using Newtonsoft.Json;
using Scanner.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Scanner
{
    class Nmap
    {
        public static Dictionary<IPAddress, string> scans = new Dictionary<IPAddress, string>();

        public void ProcessWorkItem(WorkItem item)
        {
            string workDir = Path.Combine(ConfigurationManager.AppSettings["WorkDir"], item.Id.ToString());
            var hostEntry = Dns.GetHostEntry(item.Parameters[0]);

            if (hostEntry.AddressList.Length > 0)
            {
                Directory.CreateDirectory(workDir);

                var ip = hostEntry.AddressList[0];
                if (scans.ContainsKey(ip) && File.Exists(ConfigurationManager.AppSettings["WorkDir"] + "\\" + scans[ip] + "\\scan.xml"))
                {
                    byte[] d=File.ReadAllBytes(ConfigurationManager.AppSettings["WorkDir"]+"\\"+ scans[ip]+"\\scan.xml");
                    File.WriteAllBytes(workDir + "\\scan.xml", d);
                    Console.WriteLine("Scan skipped!!");
                }
                else
                {
                    Console.WriteLine(RunCommand(ConfigurationManager.AppSettings["NmapPath"] + " -p 80,443,8080-8089 -Pn --host-timeout 30s -oX " + workDir + "\\scan.xml " + item.Parameters[0]));
                    scans.Add(ip, item.Id.ToString());
                }
            }
            File.WriteAllText(Path.Combine(workDir, "workitem.json"), JsonConvert.SerializeObject(item));
        }


        string RunCommand(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(result);
            return result;
        }
    }
}
