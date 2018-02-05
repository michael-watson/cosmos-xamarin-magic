using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using LiteDB;

using Microsoft.Azure.Documents;

namespace CosmosMagic
{
    public class SyncClient
    {
        public SyncClient()
        {
            if (string.IsNullOrWhiteSpace(CosmosAuthKey))
                throw new Exception("You must call SyncClient.Init and provide your CosmosAuthKey");
        }

        static string CosmosAuthKey { get; set; }
        public static List<string> RegisteredTypes { get; internal set; } = new List<string>();
        public static LiteDatabase localDb { get; internal set; }

        static SyncClient instance = new SyncClient();
        public static SyncClient Instance
        {
            get => instance;
            private set => instance = value;
        }

        public static void Init(string cosmosAuthKey, string localConnectionString)
        {
            CosmosAuthKey = cosmosAuthKey;
            localDb = new LiteDatabase(localConnectionString);
        }

        public void Register<T>(string databaseId, string collectionId) where T : CosmosEntity
        {
            if (RegisteredTypes.Contains(nameof(T))) return;

            CosmosManager<T>.Manager.InitializeCollection(databaseId, collectionId, CosmosAuthKey);
            RegisteredTypes.Add(nameof(T));
        }

        public async Task PullAsync<T>() where T : CosmosEntity
        {
            var items = await CosmosManager<T>.Manager.GetItems();
            var localItemCollection = localDb.GetCollection<T>(nameof(T));
            var syncConflicts = new Dictionary<T, LiteException>();

            foreach (var item in items)
            {
                try
                {
                    localItemCollection.Insert(item);
                }
                catch (LiteException dbExcep)
                {
                    syncConflicts.Add(item, dbExcep);
                }
            }

            if (syncConflicts.Count > 0)
            {
                //Handle conflicts here
            }
        }

        public async Task PushAsync<T>() where T : CosmosEntity
        {
            var localItemCollection = localDb.GetCollection<T>(nameof(T));
            var items = localItemCollection.FindAll();
            var syncConflicts = new Dictionary<T, DocumentClientException>();

            foreach (var item in items)
            {
                try
                {
                    await CosmosManager<T>.Manager.InsertItemAsync(item);
                }
                catch (DocumentClientException docClientExcep)
                {
                    syncConflicts.Add(item, docClientExcep);
                }
            }

            if (syncConflicts.Count > 0)
            {
                //Handle conflicts here
            }
        }
    }
}