using System;
using System.IO;
using System.Linq;
using VMControl;

namespace BotCreator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            {
                var script = File.ReadAllLines(args[3]).ToList();
                var vmc = new AzureVM();
                vmc.RunCmd("azure-configuration.json", "2", args[1], args[2], (i) => Console.WriteLine(i), script);
            }
            if (args[0] == "CreateAzureVM")
            {
                var script = File.ReadAllLines(args[3]).ToList();
                var vmc = new AzureVM();

                vmc.CreateAzureWindowsVM("azure-configuration.json", "3", args[1], args[2], (i) => Console.WriteLine(i), script);

                //TODO: Scheduled task creation does not work. Change to interactive. Create selenium profile to firefox and unzip profile config.
            }

            if (args[0] == "DeleteAzureVM")
            {
                var vmc = new AzureVM();

                vmc.DeleteAzureWindowsVM("azure-configuration.json", "3");
            }
        }
    }
}
