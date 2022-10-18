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
            var existingCollections = Database.ListCollections().ToList().Select(i => i["name"].AsString).ToList();

            EnsureCollectionCreated<WorkItem>(existingCollections, new Expression<Func<WorkItem, object>>[] { i => i.Id, i => i.Created });
            EnsureCollectionCreated<Pin>(existingCollections, new Expression<Func<Pin, object>>[] { i => i.Id, i => i.Created });
            EnsureCollectionCreated<ScheduledWorkItem>(existingCollections, new Expression<Func<ScheduledWorkItem, object>>[] { i => i.Id, i => i.Created });
            EnsureCollectionCreated<FilteredIP>(existingCollections, new Expression<Func<FilteredIP, object>>[] { i => i.Id });
            EnsureCollectionCreated<SlowIP>(existingCollections, new Expression<Func<SlowIP, object>>[] { i => i.Id, i => i.Created });
            EnsureCollectionCreated<User>(existingCollections, new Expression<Func<User, object>>[] { i => i.Id });
            EnsureCollectionCreated<Setting>(existingCollections, i =>
            {
                i.Indexes.CreateOne(new CreateIndexModel<Setting>(Builders<Setting>.IndexKeys.Ascending(i => i.Id)));
                i.Indexes.CreateOne(new CreateIndexModel<Setting>(Builders<Setting>.IndexKeys.Ascending(i => i.Created)));
                var indexDefinition = Builders<Setting>.IndexKeys.Combine(
                    Builders<Setting>.IndexKeys.Ascending(f => f.Module),
                    Builders<Setting>.IndexKeys.Ascending(f => f.Name));
                i.Indexes.CreateOne(new CreateIndexModel<Setting>(indexDefinition));
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


        public WorkItem PopWorkItem(string instance)
        {

            var result = Database.GetCollection<WorkItem>("WorkItems").Find(i => i.ScannerInstance==instance).SortByDescending(i => i.Created).FirstOrDefault();
            if (result == null)
                return null;

            Database.GetCollection<WorkItem>("WorkItems").DeleteOne(i => i.Id == result.Id);

            return result;
        }

        public WorkItem PushWorkItem(WorkItem item)
        {
            var collection = Database.GetCollection<WorkItem>("WorkItems");
            collection.InsertOne(item);
            return item;
        }

        public bool HasWorkItems(string instance)
        {
            var collection = Database.GetCollection<WorkItem>("WorkItems");
            return collection.CountDocuments(i => i.ScannerInstance==instance) > 0;
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
            var filter = Builders<T>.Filter.Eq("Id", id);
            var x = collection.ReplaceOne(filter, item);
        }

        public Setting GetSetting(string module,string name)
        {
            return Database.GetCollection<Setting>(ItemToCollectionName<Setting>()).Find(i=>i.Module==module && i.Name==name).FirstOrDefault();
        }
        public void SetSetting(string module, string name, string value)
        {
            var val = Database.GetCollection<Setting>("Settings").Find(i => i.Module == module && i.Name == name).FirstOrDefault();

            if (val == null)
                AddItem(new Setting() { Id = Guid.NewGuid(), Created = DateTime.UtcNow, Module = module, Name = name, Value = value });
            else
            {
                val.Value = value;
                UpdateItem(val);
            }
        }

        public User GetUser(string name)
        {
            var val = Database.GetCollection<User>(ItemToCollectionName<User>()).Find(i => i.Username == name).FirstOrDefault();

            return val;
        }

        public User GetUserByGuid(Guid id)
        {
            var val = Database.GetCollection<User>(ItemToCollectionName<User>()).Find(i => i.Id == id).FirstOrDefault();

            return val;
        }
    }
}
