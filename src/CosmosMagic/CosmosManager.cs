using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace CosmosMagic
{
    public class CosmosManager<T> where T : CosmosEntity
    {
        static CosmosManager<T> manager = new CosmosManager<T>();
        public static CosmosManager<T> Manager
        {
            get => manager;
            private set => manager = value;
        }

        Uri collectionLink;
        string databaseId;
        string collectionId;

        public DocumentClient Client { get; private set; }

        public void InitializeCollection(string databaseId, string collectionId, string authKey)
        {
            this.databaseId = databaseId;
            this.collectionId = collectionId;

            collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            Client = new DocumentClient(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), authKey);
        }

        public async Task<List<T>> GetAllItems()
        {
            var query = Client.CreateDocumentQuery<T>(collectionLink, new FeedOptions { MaxItemCount = -1 })
                              .AsDocumentQuery();

            var tList = new List<T>();
            while (query.HasMoreResults)
            {
                tList.AddRange(await query.ExecuteNextAsync<T>());
            }

            return tList;
        }

        public async Task<T> GetItem(string documentId)
        {
            var query = Client.CreateDocumentQuery(UriFactory.CreateDocumentUri(databaseId, collectionId, documentId))
                              .AsDocumentQuery();

            var item = await query.ExecuteNextAsync();

            return item?.FirstOrDefault();
        }

        public async Task<T> InsertItemAsync(T item)
        {
            var result = await Client.CreateDocumentAsync(collectionLink, item);
            item.id = result.Resource.Id;

            return item;
        }
    }
}