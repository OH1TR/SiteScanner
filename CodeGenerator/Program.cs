using DataModel;
using SkbKontur.TypeScript.ContractGenerator;
using System;

namespace CodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new TypeScriptGenerator(TypeScriptGenerationOptions.Default, CustomTypeGenerator.Null, new RootTypesProvider(typeof(ScheduledWorkItem)));
            generator.GenerateFiles("./output");
        }
    }
}
