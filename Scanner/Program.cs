using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Scanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;

namespace Scanner
{
    class Program
    {
        //public static Stack<WorkItem> WorkItems = new Stack<WorkItem>();
        public static List<IPAddress> Filter = new List<IPAddress>();
        public static Dictionary<IPAddress,int> SlowResponse = new Dictionary<IPAddress, int>();
            
        static void Main(string[] args)
        {
            Filter.Add(new IPAddress(new byte[] { 34, 102, 136, 180 }));


            var x = new WorkQueue().Pop();

            /*
                        var lines = File.ReadAllLines(@"C:\Users\Tommi_\Downloads\2021-08-24\date-specific-database\2021-08-20.csv");

                        foreach (var l in lines)
                        {
                            string[] tok = l.Split(',');
                            if (tok[1] == "\"domain_name\"")
                                continue;

                            WorkQueue.Instance.Push(new WorkItem() { Id = Guid.NewGuid(), Command = "Nmap",Host= tok[1].Trim('\"'), Parameters = new[] { tok[1].Trim('\"') }, PostEvents = new[] { "Nmap2Http" } });
                        }
          */
            try
            {
                foreach (var process in Process.GetProcessesByName("firefox"))
                    process.Kill();

                foreach (var process in Process.GetProcessesByName("java"))
                    process.Kill();

                System.Threading.Thread.Sleep(2000);

                if(File.Exists(ConfigurationManager.AppSettings["WorkDir"]+"\\scannedIps.dat"))
                        Nmap.scans=Serializer.ReadFromBinaryFile< Dictionary<IPAddress, string>>(ConfigurationManager.AppSettings["WorkDir"] + "\\scannedIps.dat");

                if (File.Exists(ConfigurationManager.AppSettings["WorkDir"] + "\\SlowResponse.dat"))
                    SlowResponse = Serializer.ReadFromBinaryFile<Dictionary<IPAddress, int>>(ConfigurationManager.AppSettings["WorkDir"] + "\\SlowResponse.dat");

                Nmap nmap = new Nmap();

                PageScanner ps = new PageScanner();
                ps.Init();
                WorkItem w;

                while ((w = WorkQueue.Instance.Pop()) != null)
                {
                    IPAddress ip=IPAddress.None;
                    try
                    {
                        var hostEntry = Dns.GetHostEntry(w.Host);
                        if (hostEntry.AddressList.Length > 0)
                        {
                            ip = hostEntry.AddressList[0];

                            if (Filter.Contains(ip))
                                throw new Exception("IP Filter");

                            if (SlowResponse.ContainsKey(ip) && SlowResponse[ip] > 10)
                                throw new Exception(" Slow IP");
                        }

                        if (w.Command == "Selenium")
                            ps.ProcessWorkItem(w);

                        if (w.Command == "Nmap")
                        {
                            nmap.ProcessWorkItem(w);
                            if (w.PostEvents.Contains("Nmap2Http"))
                            {
                                Nmap2Http nm2h = new Nmap2Http();
                                nm2h.Process(Path.Combine(ConfigurationManager.AppSettings["WorkDir"], w.Id.ToString()));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string workDir = Path.Combine(ConfigurationManager.AppSettings["WorkDir"], w.Id.ToString());
                        Directory.CreateDirectory(workDir);
                        File.WriteAllText(Path.Combine(workDir, "error.txt"), ex.Message + ex.StackTrace);
                        File.WriteAllText(Path.Combine(workDir, "workitem.json"), JsonConvert.SerializeObject(w));
                        Console.WriteLine(ex.Message + ex.StackTrace);

                        if (ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                        {
                            if (SlowResponse.ContainsKey(ip))
                                SlowResponse[ip] = SlowResponse[ip] + 1;
                            else
                                SlowResponse[ip] = 1;

                            Serializer.WriteToBinaryFile(ConfigurationManager.AppSettings["WorkDir"] + "\\scannedIps.dat", Nmap.scans);
                            Serializer.WriteToBinaryFile(ConfigurationManager.AppSettings["WorkDir"] + "\\SlowResponse.dat", SlowResponse);
                            Environment.Exit(1);
                        }
                    }
                }
                ps.Close();
                Serializer.WriteToBinaryFile(ConfigurationManager.AppSettings["WorkDir"] + "\\scannedIps.dat", Nmap.scans);
                Environment.Exit(123);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }
    }
}
