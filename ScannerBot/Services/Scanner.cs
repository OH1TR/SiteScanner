using DataModel;
using DataModel.ModuleInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace ScannerBot.Services
{
    class Scanner
    {
        public void Process()
        {

            var config = Scope.Services.GetRequiredService<IConfig>();
            var resolver = Scope.Services.GetRequiredService<ModuleResolver>();
            var filter = Scope.Services.GetRequiredService<Filter>();
            var sm = Scope.Services.GetRequiredService<ScannerModel>();

            sm.EnsureCreated();

            //sm.PushWorkItem(new WorkItem() { Command = "Eyewitness", Parameters = new[] { "https://microsoft.com" }, Created = DateTime.UtcNow, Id = Guid.NewGuid(), Host = "rouvali.com" });
            /*
            sm.AddItem(new ScheduledWorkItem()
            {
                Created = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                LastScheduledTime = new DateTime(2020, 1, 1),
                Interval = 86400,
                Work = new WorkItem()
                {
                    Created = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    Command = "DomainFinder",
                    Parameters = new[] { "*posti*" },
                    PostEvents = new[] { "DomainFinderPA" }
                }
            });
            */

            WorkItem w;

            while ((w = sm.PopWorkItem()) != null)
            {
                IPAddress ip = IPAddress.None;
                try
                {
                    var module = resolver.GetInstanceFor(w.Command);

                    Scope.WorkDir = Path.Combine(config.WorkDir, w.Id.ToString());
                    Directory.CreateDirectory(Scope.WorkDir);
                    File.WriteAllText(Path.Combine(Scope.WorkDir, "WorkItem.json"), JsonConvert.SerializeObject(w));

                    if (module.IPAware)
                    {
                        ip = module.GetIP(w);

                        if (ip!=IPAddress.None)
                        {
                            if (filter.IsFiltered(ip, out string reason))
                            {
                                WriteScanResult(w, "IP Filter, reason:" + (reason ?? ""), null, 1);
                                continue;
                            }

                            if (filter.IsSlowIP(ip))
                            {
                                WriteScanResult(w, "Slow IP", null, 1);
                                continue;
                            }
                        }
                    }

                    module.ProcessItem(w);

                    if (w.PostEvents != null)
                    {
                        foreach (var pe in w.PostEvents)
                        {
                            var postEvent = resolver.GetInstanceFor(pe);
                            Console.WriteLine("Executing post event:" + pe);
                            postEvent.ProcessItem(w);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + ex.StackTrace);
                    WriteScanResult(w, ex.Message, ex.StackTrace, 2);

                    if (ex.Message.Contains("The HTTP request to the remote WebDriver server for URL"))
                    {
                        filter.AddSlowIP(ip);

                        Environment.Exit(1);
                    }
                }
                Console.WriteLine("Processed " + Scope.WorkDir);
            }

            resolver.Shutdown();

            Console.WriteLine("All done");
            Environment.Exit(123);
        }

        void WriteScanResult(WorkItem item, string message, string stacktrace, int code)
        {
            var result = new ScanResult() { Id = item.Id, Created = DateTime.UtcNow, Message = message, StackTrace = stacktrace ,Code=code};
            File.WriteAllText(Path.Combine(Scope.WorkDir, "ScanResult.json"), JsonConvert.SerializeObject(result));
        }
    }
}