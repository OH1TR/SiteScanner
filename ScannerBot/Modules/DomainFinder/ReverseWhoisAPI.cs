using DataModel.ModuleInterface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ScannerBot.Modules
{
    class ReverseWhoisAPI
    {
        private const string Url = "https://reverse-whois.whoisxmlapi.com/api/v2";
        private IConfig _config;

        public ReverseWhoisAPI(IConfig config)
        {
            _config = config;
        }

        public string QueryDomainPart(string query, DateTime since)
        {
            var SearchParamsAdvanced =
            @"{
                advancedSearchTerms: [
                    {
                        field: 'DomainName',
                        term: '" + query + @"'
                    }
                ],
                sinceDate: '" + since.ToString("yyyy-MM-dd") + @"',
                mode: 'purchase',
                apiKey: 'API_KEY'
            }";

            var responsePost = SendPostReverseWhois(SearchParamsAdvanced);
            return responsePost;
        }

        private void PrintResponse(string response)
        {
            dynamic responseObject = JsonConvert.DeserializeObject(response);

            if (responseObject != null)
            {
                Console.Write(responseObject);
                Console.WriteLine("--------------------------------");
                return;
            }

            Console.WriteLine(response);
            Console.WriteLine();
        }

        private string SendPostReverseWhois(string SearchParams)
        {
            Console.Write("Sending request to: " + Url + "\n");

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var searchParams = SearchParams;

            using (var streamWriter =
                        new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                var json = searchParams.Replace("API_KEY", _config.GetSetting(typeof(DomainFinder), "ApiKey"));
                var jsonData = JObject.Parse(json).ToString();

                streamWriter.Write(jsonData);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var res = "";

            using (var response = (HttpWebResponse)httpWebRequest.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null || responseStream == Stream.Null)
                {
                    return res;
                }

                using (var streamReader = new StreamReader(responseStream))
                {
                    res = streamReader.ReadToEnd();
                }

                return res;
            }
        }
    }
}
