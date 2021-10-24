using DataModel.ModuleInterface;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScannerBot.Services
{
    class ModuleItem
    {
        public string Name;
        public Type Type;
        public IModule Instance;
    }

    class ModuleResolver
    {
        List<ModuleItem> Modules = new List<ModuleItem>();

        public ModuleResolver()
        {
            DiscoverAssembly(System.Reflection.Assembly.GetEntryAssembly());
        }

        public void DiscoverFolder(string path)
        {
            var files=Directory.GetFiles(Path.Combine(path, "*.dll"));
            foreach (var f in files)
                DiscoverAssembly(f);
        }

        void DiscoverAssembly(Assembly ass)
        {
            foreach (TypeInfo ti in ass.DefinedTypes)
            {
                if (ti.ImplementedInterfaces.Contains(typeof(IModule)))
                {
                    Modules.Add(new ModuleItem() { Name = ti.Name, Type = ti, Instance = null });
                }
            }
        }

        void DiscoverAssembly(string path)
        {
            Assembly ass = null;
            ass = Assembly.LoadFile(path);
            DiscoverAssembly(ass);
        }

        public IModule GetInstanceFor(string moduleName)
        {
            var result = Modules.FirstOrDefault(i => i.Name == moduleName);
            if(result.Instance==null)
            {
                result.Instance = (IModule)ActivatorUtilities.CreateInstance(Scope.Services, result.Type);
                result.Instance.Init();
                return result.Instance;
            }
            return null;
        }
        public void Shutdown()
        {
            foreach(var m in Modules)
            {
                if(m.Instance!=null)
                {
                    m.Instance.Shutdown();
                    m.Instance = null;
                }
            }
        }
    }
}
