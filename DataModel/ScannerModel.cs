using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataModel
{
    public class ScannerModel
    {
        const int PinMinutes = 10;
        public enum Driver { Mongo };

        MongoClient Client;
        IMongoDatabase Database;

        public ScannerModel(Driver driver, string conStr)
        {
            Client = new MongoClient(conStr);
            Database = Client.GetDatabase("SiteScanner");
        }

        public void EnsureCreated()
        {
            var col = Database.ListCollections().ToList().Select(i=>i["name"].AsString).ToList();

            EnsureCollectionCreated<WorkItem>(col,new Expression<Func<WorkItem, object>>[] {i=>i.Id, i => i.Created });
            EnsureCollectionCreated<Pin>(col, new Expression<Func<Pin, object>>[] { i => i.Id, i => i.Created });
            EnsureCollectionCreated<ScheduledWorkItem>(col, new Expression<Func<ScheduledWorkItem, object>>[] { i => i.Id, i => i.Created });
            EnsureCollectionCreated<FilteredIP>(col, new Expression<Func<FilteredIP, object>>[] { i => i.Id });
            EnsureCollectionCreated<SlowIP>(col, new Expression<Func<SlowIP, object>>[] { i => i.Id, i => i.Created });
        }

        void EnsureCollectionCreated<T>(List<string> existing, Expression<Func<T, object>>[] index)
        {
            string collectionName=ItemToCollectionName<T>();

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

        public WorkItem PopWorkItem()
        {

            var result = Database.GetCollection<WorkItem>("WorkItems").Find(I => true).SortByDescending(i => i.Created).FirstOrDefault();
            if (result == null)
                return null;

            Database.GetCollection<WorkItem>("WorkItems").DeleteOne(i => i.Id == result.Id);

            return result;
        }

        public void PushWorkItem(WorkItem item)
        {
            var collection = Database.GetCollection<WorkItem>("WorkItems");
            collection.InsertOne(item);
        }

        public void SetScannerPin(bool set)
        {
            if (set)
            {
                Pin pin = new Pin() { Created = DateTime.UtcNow };
                var collection = Database.GetCollection<Pin>("Pin");
                collection.InsertOne(pin);
            }
            else
            {
                Database.GetCollection<Pin>("Pin").DeleteMany(i => true);
            }
            DateTime delTime = DateTime.UtcNow.AddMinutes(PinMinutes);

            Database.GetCollection<Pin>("Pin").DeleteMany(i => i.Created < delTime);
        }

        public bool HasPin()
        {
            var i = Database.GetCollection<Pin>("Pin").Find(I => true).SortByDescending(i => i.Created).FirstOrDefault();
            return i != null && i.Created > DateTime.UtcNow.AddMinutes(PinMinutes);
        }

        public List<ScheduledWorkItem> GetScheduledWorkItems()
        {
            var result = Database.GetCollection<ScheduledWorkItem>("ScheduledWorkItems").Find(I => true).ToList();

            return result;
        }

        public void AddScheduledWorkItem(ScheduledWorkItem item)
        {
            var collection = Database.GetCollection<ScheduledWorkItem>("ScheduledWorkItems");
            collection.InsertOne(item);
        }

        public void UpdateScheduledWorkItem(ScheduledWorkItem item)
        {
            var collection = Database.GetCollection<ScheduledWorkItem>("ScheduledWorkItems");
            collection.ReplaceOne(i => i.Id == item.Id, item);
        }

        string ItemToCollectionName<T>()
        {
            return typeof(T).Name + "s";
        }
        public List<T> GetAllItems<T>()
        {
            var collectionName = ItemToCollectionName<T>();
            var result = Database.GetCollection<T>(collectionName).Find(i => true).ToList();

            return result;
        }

        public T AddItem<T>(T item)
        {
            var collectionName = ItemToCollectionName<T>();
            var collection = Database.GetCollection<T>(collectionName);
            collection.InsertOne(item);
            return (item);
        }

        public void UpdateItem<T>(T item)
        {
            var collectionName = ItemToCollectionName<T>();
            var collection = Database.GetCollection<T>(collectionName);

            var prop = typeof(T).GetProperty("Id");
            Guid id = (Guid)prop.GetValue(item);

            collection.ReplaceOne(new BsonDocument("Id", id), item);
        }
    }
}
