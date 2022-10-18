using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Azure.Management.Compute.Fluent.VirtualMachineScaleSet.Definition;

namespace VMControl
{
    public class AzureVM
    {
        public void RunCmd(string credentialsFile, string name, string username, string password, Action<string> log, List<string> script = null)
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromFile(credentialsFile);

            var azure = Azure.Authenticate(credentials).WithDefaultSubscription();

            var cred = new List<RunCommandInputParameter>();
            cred.Add(new RunCommandInputParameter("username", username));
            cred.Add(new RunCommandInputParameter("password", password));

            azure.VirtualMachines.RunPowerShellScript("RGScannerBot3", "VMScanBot3", script, cred);

        }

        public void DeleteAzureWindowsVM(string credentialsFile, string name)
        {
            var groupName = "RGScannerBot" + name;

            var credentials = SdkContext.AzureCredentialsFactory.FromFile(credentialsFile);
            var azure = Azure.Authenticate(credentials).WithDefaultSubscription();
            azure.ResourceGroups.DeleteByName(groupName);
        }

        public void CreateAzureWindowsVM(string credentialsFile, string name, string username, string password, Action<string> log, List<string> script = null)
        {
            // declare variables
            var groupName = "RGScannerBot" + name;
            var vmName = "VMScanBot" + name;
            var location = Region.EuropeNorth;
            var vNetName = "VNET-ScanBot" + name;
            var vNetAddress = "172.16.0.0/16";
            var subnetName = "Subnet-ScanBot" + name;
            var subnetAddress = "172.16.0.0/24";
            var nicName = "NIC-ScanBot" + name;
            var adminUser = username;
            var adminPassword = password;
            var publicIPName = "IP-ScanBot" + name;
            var nsgName = "NSG-ScanBot" + name;


            var credentials = SdkContext.AzureCredentialsFactory.FromFile(credentialsFile);

            var azure = Azure.Authenticate(credentials).WithDefaultSubscription();


            log($"Creating resource group {groupName} ...");
            var resourceGroup = azure.ResourceGroups.Define(groupName)
                .WithRegion(location)
                .Create();

            //Every virtual machine needs to be connected to a virtual network.
            log($"Creating virtual network {vNetName} ...");
            var network = azure.Networks.Define(vNetName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithAddressSpace(vNetAddress)
                .WithSubnet(subnetName, subnetAddress)
                .Create();


            //You need a public IP to be able to connect to the VM from the Internet
            log($"Creating public IP {publicIPName} ...");
            var publicIP = azure.PublicIPAddresses.Define(publicIPName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .Create();


            //You need a network security group for controlling the access to the VM
            log($"Creating Network Security Group {nsgName} ...");
            var nsg = azure.NetworkSecurityGroups.Define(nsgName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .Create();

            //You need a security rule for allowing the
            //Internet
            log($"Creating a Security Rule for allowing the remote");
            nsg.Update()
                .DefineRule("Allow-RDP")
                .AllowInbound()
                .FromAnyAddress()
                .FromAnyPort()
                .ToAnyAddress()
                .ToPort(3389)
                .WithProtocol(SecurityRuleProtocol.Tcp)
                .WithPriority(100)
                .Attach()
                .Apply();
            IWithManagedDiskOptionals d;

            log($"Creating network interface {nicName} ...");
            var nic = azure.NetworkInterfaces.Define(nicName)
                     .WithRegion(location)
                     .WithExistingResourceGroup(groupName)
                     .WithExistingPrimaryNetwork(network)
                     .WithSubnet(subnetName)
                     .WithPrimaryPrivateIPAddressDynamic()
                     .WithExistingPrimaryPublicIPAddress(publicIP)
                     .WithExistingNetworkSecurityGroup(nsg)
                     .Create();


            log($"Creating virtual machine {vmName} ...");
            var vm = azure.VirtualMachines.Define(vmName)
                    .WithRegion(location)
                    .WithExistingResourceGroup(groupName)
                    .WithExistingPrimaryNetworkInterface(nic)
                    .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2022-datacenter-azure-edition-smalldisk")
                    .WithAdminUsername(adminUser)
                    .WithAdminPassword(adminPassword)
                    .WithComputerName(vmName)
                    .WithOSDiskStorageAccountType(StorageAccountTypes.StandardSSDLRS)
                    .WithSize(VirtualMachineSizeTypes.StandardB2ms)
                    .WithTag("SiteScanner", "true")
                    .Create();

            if (script != null)
            {
                while (CheckVMStatus(azure, vm.Id) != PowerState.Running)
                {
                    log("Waiting VM...");
                    Thread.Sleep(3000);
                }
                Thread.Sleep(3000);
            }

            var cred = new List<RunCommandInputParameter>();
            cred.Add(new RunCommandInputParameter("username",username));
            cred.Add(new RunCommandInputParameter("password", password));
            vm.RunPowerShellScript(script, cred);
            log("Successfully created a new VM: " + vmName + " !");

        }

        private PowerState CheckVMStatus(IAzure azure, string vmID)
        {
            PowerState state = azure.VirtualMachines.GetById(vmID).PowerState;

            return state;
        }

        private PowerState ShutDownVM(IAzure azure, string vmID)
        {
            azure.VirtualMachines.GetById(vmID).PowerOff();

            PowerState state = azure.VirtualMachines.GetById(vmID).PowerState;

            return state;
        }
    }
}
