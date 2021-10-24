using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DataModel.ModuleInterface
{
    public interface IModule
    {
        public void Init();

        public void Shutdown();

        public void ProcessItem(WorkItem item);

        public bool IPAware { get; }

        public IPAddress GetIP(WorkItem item);
    }
}
