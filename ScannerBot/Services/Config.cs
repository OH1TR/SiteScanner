using DataModel.ModuleInterface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScannerBot.Services
{
    public class Config : IConfig
    {
        IConfigurationRoot _configuration;
        public Config()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            _configuration = builder.Build();
        }
        public string WorkDir { get { return _configuration["WorkDir"]; } }
        public string ConnectionString { get { return _configuration["ConnectionString"]; } }

        public string GetSetting(string module,string name)
        {
            return _configuration[module + ":" + name];
        }

        public string GetSetting(Type t,string name)
        {
            return GetSetting(t.Name, name);
        }
    }
}
