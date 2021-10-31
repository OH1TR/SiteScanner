using DataModel;
using DataModel.AdvancedServiceProvider;
using DataModel.ModuleInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScannerBot.Modules.Screenshot;
using ScannerBot.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ScannerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            //https://stackoverflow.com/questions/56802715/firefoxwebdriver-no-data-is-available-for-encoding-437
            CodePagesEncodingProvider.Instance.GetEncoding(437);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);


            var services = new ServiceCollection();
            services.AddSingleton<IConfig, Config>();
            services.AddSingleton(i => new ScannerModel(ScannerModel.Driver.Mongo, i.GetRequiredService<IConfig>().ConnectionString));
            services.AddSingleton<ILog, Log>();
            services.AddSingleton<ModuleResolver>();
            services.AddSingleton<Filter>();
            services.AddSingleton<ShellCommandRunner>();
            services.AddSingleton<Scheduler>();
            services.AddSingleton<Scanner>();
            Scope.Services = services.BuildServiceProvider(true);

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    var shell = Scope.Services.GetService<ShellCommandRunner>();
                    shell.RunCommand("kill java");
                    shell.RunCommand("kill firefox");
                }
                else
                {
                    foreach (var process in Process.GetProcessesByName("firefox"))
                        process.Kill();

                    foreach (var process in Process.GetProcessesByName("java"))
                        process.Kill();
                }
            }

            //Scope.Services.GetRequiredService<ScannerModel>().SetSetting("VMControl", "ApiKey", Guid.NewGuid().ToString());
            //Scope.Services.GetRequiredService<ScannerModel>().SetSetting("VMControl", "WebSite", "https://scannerwebsitexxx.azurewebsites.net");

            Scope.Services.GetRequiredService<ScannerModel>().EnsureCreated();

            Scheduler s = Scope.Services.GetRequiredService<Scheduler>();
            s.Process();

            Scanner sc = Scope.Services.GetRequiredService<Scanner>();
            sc.Process();
        }
    }
}
