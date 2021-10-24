using DataModel;
using DataModel.AdvancedServiceProvider;
using DataModel.ModuleInterface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScannerBot.Services;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ScannerBot
{
    class Program
    {
        static void Main(string[] args)
        {
                var services = new ServiceCollection();
            services.AddSingleton<IConfig, Config>();
            services.AddSingleton(i => new ScannerModel(ScannerModel.Driver.Mongo, i.GetRequiredService<IConfig>().ConnectionString));
            services.AddSingleton<ILog, Log>();
            services.AddSingleton<ModuleResolver>();
            services.AddSingleton<Filter>();
            services.AddSingleton<ShellCommandRunner>();
            Scope.Services = services.BuildServiceProvider(true);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var shell=Scope.Services.GetService<ShellCommandRunner>();
                shell.RunCommand("kill java");
                shell.RunCommand("kill firefox");
            }

            Scanner sc = new Scanner();
            sc.Process();

        }
    }
}
