using CommandLine;
using DataModel;
using System;
using Microsoft.Extensions.Configuration;
using DataModel.Archive;

namespace botcmd
{
    internal class Program
    {
        public static IConfiguration Configuration;

        static void Main(string[] args)
        {
            try
            {

                Configuration = new ConfigurationBuilder()
                   .AddJsonFile("appsettings.json", true, true)
                   .Build();

                var model = new ScannerModel(ScannerModel.Driver.Mongo, Configuration["ConnectionString"]);
                model.EnsureCreated();

                Parser.Default.ParseArguments<CmdOptions>(args)
                       .WithParsed(o =>
                        {
                            if (o.Command.ToLower() == "amass")
                            {
                                var item=model.PushWorkItem(new WorkItem() { Command = "Amass", Parameters = new[] { o.Target }, Created = DateTime.UtcNow, Id = Guid.NewGuid(), PostEvents = null, ScannerInstance = "Linux" });
                                Console.WriteLine("Added amass item");
                            }
                            if (o.Command.ToLower() == "screenshot")
                            {
                                var item = model.PushWorkItem(new WorkItem() { Command = "Screenshot", Parameters = new[] { "https://www.whatismybrowser.com/detect/what-http-headers-is-my-browser-sending" },Host= "www.whatismybrowser.com", Created = DateTime.UtcNow, Id = Guid.NewGuid(), PostEvents = null, ScannerInstance = "Windows" });
                                Console.WriteLine("Added screenshot item");
                            }
                        });
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.ToString()); 
            }
        }
    }
}
