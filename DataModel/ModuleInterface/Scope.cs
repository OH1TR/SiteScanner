using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.ModuleInterface
{
    public static class Scope
    {
        [ThreadStatic]
        public static string WorkDir;

        public static ServiceProvider Services;
    }
}