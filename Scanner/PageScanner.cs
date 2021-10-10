using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using Scanner.Model;
using Newtonsoft.Json;

namespace Scanner
{
    class PageScanner
    {
        public MobProxy mb;

        WebDriver wd;

        public void Init()
        {
            mb = new MobProxy();
            wd = new WebDriver(mb.GetProxy());
        }

        public void Close()
        {
            wd.Close();
            mb.Terminate();
        }

        public void ProcessWorkItem(WorkItem item)
        {
            Console.WriteLine("Url:" + item.Parameters[0]);
            string workDir = Path.Combine(ConfigurationManager.AppSettings["WorkDir"], item.Id.ToString());
            Directory.CreateDirectory(workDir);
            mb.NewHar(item.Id.ToString().Replace("-",""));
            wd.OpenURL(item.Parameters[0]);
            wd.Screenshot(Path.Combine(workDir, "image.jpg"));
            mb.SaveResultToFile(Path.Combine(workDir, "traffic.har"));
            File.WriteAllText(Path.Combine(workDir, "workitem.json"), JsonConvert.SerializeObject(item));
        }
    }
}
