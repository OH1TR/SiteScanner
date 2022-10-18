using System;
using Azure.Core;
using Azure.Identity;
using DataModel;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;


namespace VMStartFunction
{
    public static class Function1
    {
        const string WindowsInstanceName = "Windows";

        [FunctionName("VMStartFunction")]
        public static void Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

                ScannerModel model = new ScannerModel(ScannerModel.Driver.Mongo, Environment.GetEnvironmentVariable("ConnectionString", EnvironmentVariableTarget.Process));

                if (HasSheduledTasks(model, WindowsInstanceName))
                {
                    log.LogInformation($"New tasks, starting..");
                    StartVM(log);
                    return;
                }

                if (model.HasWorkItems(WindowsInstanceName))
                {
                    log.LogInformation($"WorkItems starting..");
                    StartVM(log);
                    return;
                }
            }
            catch(Exception ex)
            {
                log.LogError(ex, "VMStartFunction");
            }
        }

        static bool HasSheduledTasks(ScannerModel model,string Instance)
        {
            DateTime now = DateTime.UtcNow;

            var items = model.GetAllItems<ScheduledWorkItem>();
            foreach (var item in items)
            {
                if (item.LastScheduledTime < now && item.Work.ScannerInstance==Instance)
                {
                    return true;
                }
            }
            return false;
        }

        static void StartVM(ILogger log)
        {
            var defaultCredential = new DefaultAzureCredential();
            var defaultToken = defaultCredential.GetToken(new TokenRequestContext(new[] { "https://management.azure.com/.default" })).Token;
            var defaultTokenCredentials = new Microsoft.Rest.TokenCredentials(defaultToken);
            var azureCredentials = new Microsoft.Azure.Management.ResourceManager.Fluent.Authentication.AzureCredentials(defaultTokenCredentials, defaultTokenCredentials, null, AzureEnvironment.AzureGlobalCloud);
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(azureCredentials).WithSubscription(Environment.GetEnvironmentVariable("Subscription", EnvironmentVariableTarget.Process));
            System.Collections.Generic.IEnumerable<IVirtualMachine> vms = azure.VirtualMachines.List();
            foreach (var vm in vms)
            {
                if (vm.Tags.ContainsKey("SiteScanner"))
                {
                    if (vm.PowerState != PowerState.Running)
                    {
                        log.LogInformation($"Starting VM...");
                        vm.Start();
                    }
                    else
                        log.LogInformation($"Already running.");
                }
                else
                    log.LogInformation($"Cannot find VM, no SiteScanner tag.");
            }
        }
    }
}
