using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;

namespace S3RabbitMongo.Database.Mongo
{
    public class MongoDocument : Document
    {
        public ObjectId Id { get; set; }
        public MongoDocument(Document document) : base(document)
        {
        }
    }
    
    public class Document
    {
        public string JobId { get; set; }
        public string DocumentId { get; set; }
        public string Data { get; set; }
        
        public Document(string jobId, string documentId, string data)
        {
            JobId = jobId;
            DocumentId = documentId;
            Data = data;
        }
        
        public Document(Document document)
        {
            DocumentId = document.DocumentId;
            Data = document.Data;
            JobId = document.JobId;
        }
    }

    [ServiceConfiguration(ServiceName = "document_store", ServiceType = "mongo", ServiceInterface = typeof(IDocumentStore))]
    public class MongoDocumentStore : IDocumentStore
    {
        private readonly ILogger<MongoDocumentStore> _logger;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<MongoDocument> _collection;
        
        public MongoDocumentStore(ILogger<MongoDocumentStore> logger, IMongoClient mongoClient,
            IOptions<MongoDocumentStoreOptions> options)
        {
            _logger = logger;
            _database = mongoClient.GetDatabase(options.Value.Database);
            CreateCollectionIfNotExists(options.Value.Collection);
            _collection = _database.GetCollection<MongoDocument>(options.Value.Collection);
        }

        private void CreateCollectionIfNotExists(string collectionName)
        {
            _database.CreateCollection(collectionName);
        }
    
        public void AddDocument(Document document)
        {
            _collection.InsertOne(new MongoDocument(document));
        }

        public Document RetrieveDocument(string documentId)
        {
            return _collection.Find(Builders<MongoDocument>.Filter.Eq("DocumentId", documentId)).First();
        }
    }
}