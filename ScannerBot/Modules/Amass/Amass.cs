using DataModel;
using DataModel.ModuleInterface;
using ScannerBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ScannerBot.Modules
{
    class Amass : IModule
    {
        private ScannerModel _model;
        private IConfig _config;
        ShellCommandRunner _commandRunner;

        public bool IPAware => false;

        public Amass(ScannerModel model, IConfig config, ShellCommandRunner commandRunner)
        {
            _model = model;
            _config = config;
            _commandRunner = commandRunner;
        }

        public void Init()
        {

        }

        public void Shutdown()
        {

        }

        public void ProcessItem(WorkItem item)
        {
            string resultPath = Path.Combine(Scope.WorkDir, "result.json");
            Console.WriteLine(_commandRunner.RunCommand("amass  enum -json " + resultPath + " -d " + item.Parameters[0]));
        }

        public IPAddress GetIP(WorkItem item)
        {
            throw new NotImplementedException();
        }
    }
}
