using AutoMapper;
using DataModel.Archive;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace Archiver
{
    class tmp
    {
        public AutomatedTester.BrowserMob.HAR.Log Log { get; set; }
    }

    public class ArchiveDBWorker
    {
        private IConfiguration _configuration;
        ArchiveModel _model;
        IMapper mapper;
        Dictionary<string, string> words = new Dictionary<string, string>();
        JsonSerializer _serializer = new JsonSerializer();

        public ArchiveDBWorker()
        {
            _configuration = Program.Configuration;

            _model = new ArchiveModel(ArchiveModel.Driver.Mongo, _configuration["ArchiveDB"]);
            _model.EnsureCreated();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AutomatedTester.BrowserMob.HAR.Entry, HTTPTransaction>();
                cfg.CreateMap<AutomatedTester.BrowserMob.HAR.Request, Request>();
                cfg.CreateMap<AutomatedTester.BrowserMob.HAR.Response, Response>();
                cfg.CreateMap<AutomatedTester.BrowserMob.HAR.Header, Header>();
            });

            mapper = config.CreateMapper();
        }

        public void ProcessFolder()
        {

            var files = Directory.GetFiles(_configuration["StorageDir"]);
            int c = 0;
            foreach (var file in files)
            {
                Console.WriteLine(c.ToString() + "/" + files.Length);
                ProcessArchive(file);
            }
        }

        public void ProcessArchive(string path)
        {
            using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                    if (entry.Name == "WorkItem.json")
                        ProcessWorkItemFile(zip, Path.GetFileName(path), entry);

        }

        public void ProcessWorkItemFile(ZipArchive zip, string archive, ZipArchiveEntry entry)
        {
            string folderInZip = entry.FullName.Split('/')[0];
            var wi = DeserializeFromEntry<DataModel.WorkItem>(entry);
            Scan scan = new Scan();
            scan.Id = Guid.NewGuid();
            scan.Created = DateTime.UtcNow;
            scan.WorkItem = wi;
            scan.ArchivePath = archive + folderInZip;
            _model.AddItem(scan);

            if (scan.WorkItem.Command == "Screenshot")
            {
                var harEntry = zip.GetEntry(Path.Combine(folderInZip, "traffic.har"));
                if (harEntry != null)
                    ProcessHarFile(archive, harEntry);
            }
            else if (scan.WorkItem.Command == "Amass")
            {
                var result = zip.GetEntry(folderInZip+ "/result.json");
                if(result != null)
                {
                    ProcessAmassEntry(archive, result);
                }
            }

        }

        public void ProcessAmassEntry(string archive, ZipArchiveEntry entry)
        {
            var lines = ReadAllLines(entry);
            foreach(var line in lines)
            {
                var sub=JsonConvert.DeserializeObject<SubDomain>(line);
                if (_model.GetSubDomain(sub.Name) == null)
                {
                    sub.Id = Guid.NewGuid();
                    sub.Created = DateTime.UtcNow;
                    _model.AddItem(sub);
                }
            }
        }

        public string[] ReadAllLines(ZipArchiveEntry entry)
        {
            using(var stream = entry.Open())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public T DeserializeFromEntry<T>(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
            using (StreamReader reader = new StreamReader(stream))
            {
                return (T)_serializer.Deserialize(reader, typeof(T));
            }
        }

        public void ProcessHarFile(string archive, ZipArchiveEntry entry)
        {
            Guid archiveId = Guid.Parse(archive);
            Guid scanId = Guid.Parse(entry.FullName.Split('/')[0]);
            Console.WriteLine("Processing:" + archiveId.ToString() + "/" + scanId.ToString());
            tmp har = DeserializeFromEntry<tmp>(entry);

            foreach (var i in har.Log.Entries)
            {
                var a = (HTTPTransaction)mapper.Map(i, typeof(AutomatedTester.BrowserMob.HAR.Entry), typeof(HTTPTransaction));
                a.Created = DateTime.UtcNow;
                a.ArchiveId = archiveId;
                a.ScanId = scanId;
                a.Id = Guid.NewGuid();
                //model.AddItem(a);

                if (i.Request.Url.EndsWith(".css") && i.Response.Content.Size > 0)
                {
                    var data = ScanString(i.Response.Content.Text);

                    foreach (var item in data.strings)
                    {
                        if (!words.ContainsKey(item))
                            words.Add(item, item);
                    }

                }
                if (i.Request.Url.EndsWith(".js") && i.Response.Content.Size > 0)
                {
                    if (i.Response.Content.Encoding == "base64")
                    {
                        /*
                        byte[] data = Convert.FromBase64String(i.Response.Content.Text);
                        string str = Encoding.UTF8.GetString(data);
                        */
                        string str = DecompressWithBrotli(i.Response.Content.Text);
                        ScanJSFile(str);
                    }
                    else
                        ScanJSFile(i.Response.Content.Text);
                }
            }

            /*
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                var har = (tmp)serializer.Deserialize(file, typeof(tmp));

            }
            */
        }

        public static string DecompressWithBrotli(string b64)
        {
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(b64));
            MemoryStream decompressedStream = new MemoryStream();
            using (BrotliStream decompressionStream = new BrotliStream(ms, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(decompressedStream);
            }
            decompressedStream.Position = 0;
            return Encoding.UTF8.GetString(decompressedStream.ToArray());
        }

        public void ScanJSFile(string content)
        {
            File.WriteAllText("c:\\temp\\t.js", content, Encoding.UTF8);
            Directory.SetCurrentDirectory("c:\\temp");
            var pro = System.Diagnostics.Process.Start("node.exe", "c:\\temp\\test.js");
            pro.WaitForExit();
            if (pro.ExitCode != 0)
                Console.WriteLine("ERROR");

            var data = JsonConvert.DeserializeObject<List<JSToken>>(File.ReadAllText("c:\\temp\\out.json"));

            foreach (var token in data)
            {

                if (token.type == "String")
                {
                    double e = ShannonEntropy(token.value);
                    var strData = ScanString(token.value);
                    int c = 0;
                    foreach (var item in strData.strings)
                    {
                        if (words.ContainsKey(item))
                            c += item.Length;
                    }
                    c += strData.separators + strData.spaces;
                    double css = 1 - (((double)c) / ((double)token.value.Length));
                    if (e > 4.0 && css > 0.7)
                        Console.WriteLine(css.ToString() + ":" + e + ":" + token.value);
                }
            }
        }

        public class ScanData
        {
            public List<string> strings;
            public int spaces;
            public int separators;
            public int letters;
            public int numbers;
        }

        public ScanData ScanString(string str)
        {
            ScanData data = new ScanData();
            data.strings = new List<string>();

            string current = "";

            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsLetter(str[i]))
                {
                    current += str[i];
                    data.letters++;
                }
                else if (char.IsDigit(str[i]))
                {
                    current += str[i];
                    data.numbers++;
                }
                else
                {
                    if (current.Length > 2)
                    {
                        if (!char.IsDigit(current[0]))
                            data.strings.Add(current);
                        current = "";
                    }
                    if (" \t".Contains(str[i]))
                        data.spaces++;

                    if (",.<>-+=(){}[]/*-+'\"".Contains(str[i]))
                        data.separators++;
                }
            }

            if (current.Length > 2)
                data.strings.Add(current);

            if (data.strings.Count > 0)
                data.strings = data.strings.Distinct().Select(i => i.ToLower()).ToList();

            return data;
        }

        public void RunCmd()
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C node  c:\\temp\\test.js";
            process.StartInfo = startInfo;
            process.Start();
        }

        public static double ShannonEntropy(string s)
        {
            var map = new Dictionary<char, int>();
            foreach (char c in s)
            {
                if (!map.ContainsKey(c))
                    map.Add(c, 1);
                else
                    map[c] += 1;
            }

            double result = 0.0;
            int len = s.Length;
            foreach (var item in map)
            {
                var frequency = (double)item.Value / len;
                result -= frequency * (Math.Log(frequency) / Math.Log(2));
            }

            return result;
        }
    }
}
