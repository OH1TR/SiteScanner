using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DataModel;
using DataModel.Authentication;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Archiver
{
    class Downloader
    {
        IConfiguration _configuration;

        public Downloader()
        {
            _configuration = Program.Configuration;
        }

        public void Process()
        {

            BlobContainerClient containerClient = new BlobContainerClient(new Uri(_configuration["BlobSAS"]));
            
            foreach (Azure.Page<BlobItem> blobPage in containerClient.GetBlobs().AsPages())
            {
                foreach(var blob in blobPage.Values)
                {
                    var client = containerClient.GetBlobClient(blob.Name);
                    client.DownloadTo(Path.Combine(_configuration["StorageDir"], blob.Name));
                    client.Delete();
                }
            }
        }


        /*
        public void Process()
        {
            var token = Authenticate();
            var files = GetFiles(token);
            foreach (var file in files)
            {
                if (Download(token, file))
                    Delete(token, file);
            }
        }

        public string[] GetFiles(string token)
        {
            var uri = new Uri(_configuration["Website"] + "/download");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var response = client.GetAsync(uri).Result;
            return JsonConvert.DeserializeObject<string[]>(response.Content.ReadAsStringAsync().Result);
        }

        public bool Download(string token, string filename)
        {
            var uri = new Uri(_configuration["Website"] + "/download/" + filename);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var response = client.GetAsync(uri,HttpCompletionOption.ResponseHeadersRead).Result;
            using (var fs = new FileStream(Path.Combine(_configuration["StorageDir"], filename), FileMode.OpenOrCreate))
            {
                var copy=response.Content.CopyToAsync(fs);
                copy.Wait();
            }
            return response.StatusCode == HttpStatusCode.OK;

        }

        public void Delete(string token, string filename)
        {
            var uri = new Uri(_configuration["Website"] + "/download/" + filename);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var response = client.DeleteAsync(uri).Result;
        }
        string Authenticate()
        {
            AuthenticateRequest req = new AuthenticateRequest()
            {
                Username = _configuration["DataDownloader:Username"],
                Password = _configuration["DataDownloader:Password"]
            };

            HttpClient client = new HttpClient();
            var response = client.PostAsync(_configuration["Website"] + "/auth/authenticate", new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json")).Result;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var authresp = JsonConvert.DeserializeObject<AuthenticateResponse>(response.Content.ReadAsStringAsync().Result);
                return authresp.Token;
            }
            return null;
        }
        */
    }
}
