using Azure.Core;
using Azure.Identity;
using DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;
using ScannerWebSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScannerWebSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VMControlController : ControllerBase
    {
        private readonly ILogger<VMControlController> _logger;
        private readonly ScannerModel _scannerModel;

        public VMControlController(ILogger<VMControlController> logger, ScannerModel scannerModel)
        {
            _logger = logger;
            _scannerModel = scannerModel;
        }

        [HttpPost]
        public IActionResult Post([FromBody] VMControlRequest request)
        {
            var key = _scannerModel.GetSetting("VMControl", "ApiKey");

            if (key != null && request.ApiKey == key.Value)
            {
                System.Diagnostics.Trace.Write("Command:" + request.Command);
                ThreadPool.QueueUserWorkItem(i => VMControl(request.Command));
                //VMControl(request.Command);
                return Ok();
            }
            System.Diagnostics.Trace.Write("Wrong key");
            return Unauthorized();
        }

        void VMControl(string command)
        {
            var defaultCredential = new DefaultAzureCredential();
            var defaultToken = defaultCredential.GetToken(new TokenRequestContext(new[] { "https://management.azure.com/.default" })).Token;
            var defaultTokenCredentials = new Microsoft.Rest.TokenCredentials(defaultToken);
            var azureCredentials = new Microsoft.Azure.Management.ResourceManager.Fluent.Authentication.AzureCredentials(defaultTokenCredentials, defaultTokenCredentials, null, AzureEnvironment.AzureGlobalCloud);
            var azure = Microsoft.Azure.Management.Fluent.Azure.Configure().Authenticate(azureCredentials).WithSubscription(Environment.GetEnvironmentVariable("Subscription", EnvironmentVariableTarget.Process));
            IEnumerable<IVirtualMachine> vms = azure.VirtualMachines.List();

            bool found = false;
            foreach (var vm in vms)
            {
                if (vm.Tags.ContainsKey("SiteScanner"))
                {
                    found = true;

                    if (command == "start")
                    {
                        if (vm.PowerState != PowerState.Running)
                        {
                            System.Diagnostics.Trace.Write($"Starting VM...");
                            vm.Start();
                        }
                        else
                            System.Diagnostics.Trace.Write($"Already running.");
                    }

                    if (command == "stop")
                    {
                        // Some shutdown time to vm
                        Thread.Sleep(2 * 60 * 1000);
                        if (vm.PowerState != PowerState.Deallocated)
                        {
                            System.Diagnostics.Trace.Write($"Deallocating VM...");
                            vm.Deallocate();
                        }
                        else
                            System.Diagnostics.Trace.Write($"Already deallocated.");
                    }
                }
            }

            if (!found)
                System.Diagnostics.Trace.Write($"Cannot find VM, no SiteScanner tag.");
        }
    }
}
