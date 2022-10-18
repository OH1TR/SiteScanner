using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using DataModel.Archive;
using System.Linq.Expressions;
using System.Linq;

namespace DataModel.Archive
{
    public class ArchiveModel
    {
        public enum Driver { Mongo };

        MongoClient Client;
        IMongoDatabase Database;

        public ArchiveModel(Driver driver, string conStr)
        {
            Client = new MongoClient(conStr);
            Database = Client.GetDatabase("SiteScannerArchive");
        }

        public void EnsureCreated()
        {
            var existingCollections = Database.ListCollections().ToList().Select(i => i["name"].AsString).ToList();

            EnsureCollectionCreated<HTTPTransaction>(existingCollections, i =>
            {
                i.Indexes.CreateOne(new CreateIndexModel<HTTPTransaction>(Builders<HTTPTransaction>.IndexKeys.Ascending(i => i.Id)));
                i.Indexes.CreateOne(new CreateIndexModel<HTTPTransaction>(Builders<HTTPTransaction>.IndexKeys.Ascending(i => i.Created)));
                i.Indexes.CreateOne(new CreateIndexModel<HTTPTransaction>(Builders<HTTPTransaction>.IndexKeys.Ascending(i => i.ServerIpAddress)));
                var indexDefinition = Builders<HTTPTransaction>.IndexKeys.Descending("Response.Headers.Name");
                i.Indexes.CreateOne(indexDefinition);
            });

            EnsureCollectionCreated<SubDomain>(existingCollections,  i =>
            {
                i.Indexes.CreateOne(new CreateIndexModel<SubDomain>(Builders<SubDomain>.IndexKeys.Ascending(i => i.Id)));
                i.Indexes.CreateOne(new CreateIndexModel<SubDomain>(Builders<SubDomain>.IndexKeys.Ascending(i => i.Created)));
                i.Indexes.CreateOne(new CreateIndexModel<SubDomain>(Builders<SubDomain>.IndexKeys.Ascending(i => i.Name)));
                i.Indexes.CreateOne(new CreateIndexModel<SubDomain>(Builders<SubDomain>.IndexKeys.Ascending(i => i.Domain)));                
                i.Indexes.CreateOne(Builders<SubDomain>.IndexKeys.Descending("Addresses.Ip"));
                i.Indexes.CreateOne(Builders<SubDomain>.IndexKeys.Descending("Addresses.Cidr"));
                i.Indexes.CreateOne(Builders<SubDomain>.IndexKeys.Descending("Addresses.Asn"));
            });

            EnsureCollectionCreated<Scan>(existingCollections, i =>
            {
                i.Indexes.CreateOne(new CreateIndexModel<Scan>(Builders<Scan>.IndexKeys.Ascending(i => i.Id)));
                i.Indexes.CreateOne(new CreateIndexModel<Scan>(Builders<Scan>.IndexKeys.Ascending(i => i.Created)));
                i.Indexes.CreateOne(Builders<Scan>.IndexKeys.Descending("WorkItem.Command"));
                i.Indexes.CreateOne(Builders<Scan>.IndexKeys.Descending("WorkItem.Host"));
                i.Indexes.CreateOne(Builders<Scan>.IndexKeys.Descending("WorkItem.Parameters"));
            });            
        }

        void EnsureCollectionCreated<T>(List<string> existing, Expression<Func<T, object>>[] index)
        {
            string collectionName = ItemToCollectionName<T>();

            if (!existing.Contains(collectionName))
            {
                Database.CreateCollection(collectionName);
                var collection = Database.GetCollection<T>(collectionName);
                foreach (var i in index)
                {
                    var indexKeysDefinition = Builders<T>.IndexKeys.Ascending(i);
                    collection.Indexes.CreateOne(new CreateIndexModel<T>(indexKeysDefinition));
                }
            }
        }

        void EnsureCollectionCreated<T>(List<string> existing, Action<IMongoCollection<T>> indexFunc)
        {
            string collectionName = ItemToCollectionName<T>();

            if (!existing.Contains(collectionName))
            {
                Database.CreateCollection(collectionName);
                var collection = Database.GetCollection<T>(collectionName);
                indexFunc(collection);
            }
        }

        string ItemToCollectionName<T>()
        {
            return typeof(T).Name + "s";
        }

        public T AddItem<T>(T item)
        {
            var collectionName = ItemToCollectionName<T>();
            var collection = Database.GetCollection<T>(collectionName);
            collection.InsertOne(item);
            return (item);
        }

        public SubDomain GetSubDomain(string name)
        {
            var val = Database.GetCollection<SubDomain>(ItemToCollectionName<SubDomain>()).Find(i => i.Name == name).FirstOrDefault();

            return val;
        }
    }
}
