﻿using DataModel;
using DataModel.ModuleInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.IO.Compression;
using Microsoft.Azure.Storage;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

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

                        if (ip != IPAddress.None)
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

            if (!System.Diagnostics.Debugger.IsAttached)
                VMShutdown();

            Environment.Exit(123);
        }

        void WriteScanResult(WorkItem item, string message, string stacktrace, int code)
        {
            var result = new ScanResult() { Id = item.Id, Created = DateTime.UtcNow, Message = message, StackTrace = stacktrace, Code = code };
            File.WriteAllText(Path.Combine(Scope.WorkDir, "ScanResult.json"), JsonConvert.SerializeObject(result));
        }

        public void VMShutdown()
        {
            var cmdrun = Scope.Services.GetRequiredService<ShellCommandRunner>();
            var sm = Scope.Services.GetRequiredService<ScannerModel>();

            string apiKey = sm.GetSetting("VMControl", "ApiKey")?.Value;
            string webSite = sm.GetSetting("VMControl", "WebSite")?.Value;

            string req = "{\"ApiKey\":\"##KEY##\",\"Command\":\"stop\"}".Replace("##KEY##", apiKey);
            var client = new HttpClient();
            var res = client.PostAsync(webSite + "/VMControl", new StringContent(req, Encoding.UTF8, "application/json")).Result;
            Console.WriteLine("Shutdown api:" + res.StatusCode);
            cmdrun.RunCommand("Shutdown /s /f /t 1");
        }

        /*
        public async void UploadWorkDirectory(string workDir)
        {
            int size = 8000000;
            string filename = Guid.NewGuid() + ".zip";
            string tmpFile = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), filename);
            ZipFile.CreateFromDirectory(workDir, tmpFile, CompressionLevel.Fastest, false);
            var stream = new FileStream(tmpFile, FileMode.Open, FileAccess.Read);

            var _container = new CloudBlobContainer(new Uri(connectionString));
            CloudBlockBlob blob = _container.GetBlockBlobReference(filename);

            // local variable to track the current number of bytes read into buffer
            int bytesRead;

        // track the current block number as the code iterates through the file
            int blockNumber = 0;

            // Create list to track blockIds, it will be needed after the loop
            List<string> blockList = new List<string>();

            do
            {
                // increment block number by 1 each iteration
                blockNumber++;

                // set block ID as a string and convert it to Base64 which is the required format
                string blockId = $"{blockNumber:0000000}";
                string base64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockId));

                // create buffer and retrieve chunk
                byte[] buffer = new byte[size];
                bytesRead = await stream.ReadAsync(buffer, 0, size);

                // Upload buffer chunk to Azure
                await blob.PutBlockAsync(base64BlockId, new MemoryStream(buffer, 0, bytesRead), null);

                // add the current blockId into our list
                blockList.Add(base64BlockId);

                // While bytesRead == size it means there is more data left to read and process
            } while (bytesRead == size);

            // add the blockList to the Azure which allows the resource to stick together the chunks
            await blob.PutBlockListAsync(blockList);

            // make sure to dispose the stream once your are done
            stream.Dispose();
        }
        */
    }
}