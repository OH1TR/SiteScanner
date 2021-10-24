using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.ModuleInterface
{
    public interface IConfig
    {
        string WorkDir { get; }
        string ConnectionString { get; }
        string GetSetting(string module, string name);
        string GetSetting(Type t, string name);
    }
}
