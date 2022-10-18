using DataModel;
using DataModel.Authentication;
using DataModel.ModuleInterface;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Azure.Storage.Blobs;
using Azure;
using Azure.Storage.Blobs.Models;

namespace ScannerBot.Services
{
    class DataUploader
    {
        private ScannerModel _scannerModel;
        private IConfig _config;

        public DataUploader(ScannerModel scannerModel, IConfig config)
        {
            _scannerModel = scannerModel;
            _config = config;
        }

        public void UploadAndDeleteResults()
        {
            var tmpFile = Path.GetTempFileName();
            File.Delete(tmpFile);

            ZipFile.CreateFromDirectory(_config.WorkDir, tmpFile, CompressionLevel.Fastest, false);

            BlobContainerClient containerClient = new BlobContainerClient(new Uri(_config.GetSetting("DataUploader", "BlobSAS")));
            BlobClient blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString().Replace("-",""));
            blobClient.Upload(tmpFile, true);
            var dirs = Directory.GetDirectories(_config.WorkDir);
            foreach (var dir in dirs)
            {
                Directory.Delete(dir, true);
            }
            File.Delete(tmpFile);
        }
    }
}
