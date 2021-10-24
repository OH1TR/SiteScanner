using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Newtonsoft.Json;
using Scanner.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scanner
{
    class WorkQueue
    {
/*
        create table Workitems(ID uniqueidentifier,Data varchar(max),Created datetime)
        create index IX_Workitems_Created on Workitems (Created asc)
        create index IX_Workitems_ID on Workitems (ID asc)
*/

        public static WorkQueue Instance = new WorkQueue();

        public WorkItem Pop()
        {
            var mongoConnectionUrl = new MongoUrl(ConfigurationManager.AppSettings["ConnectionString"]);
            var mongoClientSettings = MongoClientSettings.FromUrl(mongoConnectionUrl);
            /*
            mongoClientSettings.ClusterConfigurator = cb => {
                cb.Subscribe<CommandStartedEvent>(e => {
                    Console.WriteLine($"{e.CommandName} - {e.Command.ToJson()}");
                });
            };
            */
            var client = new MongoClient(mongoClientSettings);

            var database = client.GetDatabase("SiteScanner");
            var result = database.GetCollection<WorkItem>("WorkItems").Find(I => true).SortByDescending(i=>i.Created).FirstOrDefault();
            database.GetCollection<WorkItem>("WorkItems").DeleteOne(i => i.Id == result.Id);

            return result;
        }

        public void Push(WorkItem item)
        {
            var client = new MongoClient(ConfigurationManager.AppSettings["ConnectionString"]);
            var database = client.GetDatabase("SiteScanner");
            var collection = database.GetCollection<BsonDocument>("WorkItems");
            collection.InsertOne(item.ToBsonDocument());
        }
    }
}
