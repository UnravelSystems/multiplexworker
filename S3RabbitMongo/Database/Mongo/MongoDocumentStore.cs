using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.MassTransit;
using S3RabbitMongo.Models;

namespace S3RabbitMongo.Database.Mongo
{
    public class MongoDocument<T1, T2> : Document<T1, T2>
    {
        public ObjectId Id { get; set; }

        public MongoDocument(Document<T1, T2> document) : base(document)
        {
        }
    }

    public class Document<T1, T2>
    {
        public string JobId { get; set; }
        public string DocumentId { get; set; }
        public T1 Data { get; set; }
        public T2 Metadata { get; set; }

        public Document(string jobId, string documentId, T1 data, T2 metadata)
        {
            JobId = jobId;
            DocumentId = documentId;
            Data = data;
            Metadata = metadata;
        }

        public Document(Document<T1, T2> document)
        {
            DocumentId = document.DocumentId;
            Data = document.Data;
            JobId = document.JobId;
            Metadata = document.Metadata;
        }
    }

    [ServiceConfiguration(ServiceName = "document_store", ServiceType = "mongo",
        ServiceInterface = typeof(IDocumentStore<Document<TreeNode<string>, string>>))]
    public class MongoDocumentStore : IDocumentStore<Document<TreeNode<string>, string>>
    {
        private readonly ILogger<MongoDocumentStore> _logger;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<MongoDocument<TreeNode<string>, string>>? _collection;

        public MongoDocumentStore(ILogger<MongoDocumentStore> logger, IMongoClient mongoClient,
            IOptions<MongoDocumentStoreOptions> options)
        {
            _logger = logger;
            MongoDocumentStoreOptions mongoOptions = options.Value;
            _database = mongoClient.GetDatabase(mongoOptions.Database);
            _collection = GetCollection(mongoOptions.Collection);
        }

        private IMongoCollection<MongoDocument<TreeNode<string>, string>>? GetCollection(string collectionName)
        {
            if (string.IsNullOrEmpty(collectionName))
            {
                return null;
            }

            CreateCollectionIfNotExists(collectionName);
            return _database.GetCollection<MongoDocument<TreeNode<string>, string>>(collectionName);
        }

        private void CreateCollectionIfNotExists(string collectionName)
        {
            _database.CreateCollection(collectionName);
        }

        public void AddDocument(string collectionName, Document<TreeNode<string>, string> document)
        {
            IMongoCollection<MongoDocument<TreeNode<string>, string>>? collection = GetCollection(collectionName);
            collection?.InsertOne(new MongoDocument<TreeNode<string>, string>(document));
        }

        public void AddDocument(Document<TreeNode<string>, string> document)
        {
            _collection!.InsertOne(new MongoDocument<TreeNode<string>, string>(document));
        }

        public Document<TreeNode<string>, string> RetrieveDocument(string documentId)
        {
            return _collection
                .Find(Builders<MongoDocument<TreeNode<string>, string>>.Filter.Eq("DocumentId", documentId)).First();
        }

        public Document<TreeNode<string>, string>? RetrieveDocument(string collectionName, string documentId)
        {
            IMongoCollection<MongoDocument<TreeNode<string>, string>>? collection = GetCollection(collectionName);
            return collection
                ?.Find(Builders<MongoDocument<TreeNode<string>, string>>.Filter.Eq("DocumentId", documentId)).First();
        }
    }
}