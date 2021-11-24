using DataModel;
using DataModel.ModuleInterface;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ScannerBot.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ScannerBot.Services
{
    class DataUploader
    {
        private ScannerModel _scannerModel;
        private IConfig _config;
        private string _webSite;

        public DataUploader(ScannerModel scannerModel, IConfig config)
        {
            _scannerModel = scannerModel;
            _config = config;
#if DEBUG
            _webSite = "http://localhost:32911";
#else
            _webSite = _scannerModel.GetSetting("VMControl", "WebSite")?.Value;
#endif
        }

        public void UploadAndDeleteResults()
        {
            var token = Authenticate();

            var tmpFile = Path.GetTempFileName();
            File.Delete(tmpFile);

            TusDotNetClient.TusClient client = new TusDotNetClient.TusClient();
            ZipFile.CreateFromDirectory(_config.WorkDir, tmpFile, CompressionLevel.Fastest, false);
            var fi = new FileInfo(tmpFile);

            client.AdditionalHeaders.Add("Authorization", "Bearer " + token);
            var r = client.CreateAsync(_webSite + "/" + Constants.STORAGEURLPART, fi).Result;
            var upres = client.UploadAsync(r, fi).Operation.Result;
            if (upres.Count == 1 && upres[0].StatusCode == HttpStatusCode.NoContent)
            {
                var dirs = Directory.GetDirectories(_config.WorkDir);
                foreach (var dir in dirs)
                {
                    Directory.Delete(dir, true);
                }
            }
            File.Delete(tmpFile);
        }

        string Authenticate()
        {
            AuthenticateRequest req = new AuthenticateRequest()
            {
                Username = _config.GetSetting("DataUploader", "Username"),
                Password = _config.GetSetting("DataUploader", "Password")
            };

            HttpClient client = new HttpClient();
            var response = client.PostAsync(_webSite + "/auth/authenticate", new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json")).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authresp = JsonConvert.DeserializeObject<AuthenticateResponse>(response.Content.ReadAsStringAsync().Result);
                return authresp.Token;
            }
            return null;
        }
    }
}
