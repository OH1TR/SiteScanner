using DataModel;
using DataModel.ModuleInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ScannerBot.Modules
{
    class DomainFinder : IModule
    {
        public bool IPAware => false;
        private IConfig _config;

        public DomainFinder(IConfig config)
        {
            _config = config;
        }

        public IPAddress GetIP(WorkItem item)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
        }

        public void ProcessItem(WorkItem item)
        {
            ReverseWhoisAPI api = new ReverseWhoisAPI(_config);
            var data = api.QueryDomainPart(item.Parameters[0], DateTime.Now.AddDays(item.Parameters.Length > 1 ? -int.Parse(item.Parameters[1]) : -3));
            File.WriteAllText(Path.Combine(Scope.WorkDir, "result.json"), data);
        }

        public void Shutdown()
        {
        }
    }
}
