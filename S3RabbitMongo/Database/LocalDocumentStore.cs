using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Database.Mongo;

namespace S3RabbitMongo.Database;

[ServiceConfiguration(ServiceName = "document_store", ServiceType = "local", ServiceInterface = typeof(IDocumentStore<Document<string, string>>))]
public class LocalDocumentStore : IDocumentStore<Document<string, string>>
{
    public void AddDocument(string collectionName, Document<string, string> document)
    {
    }

    public void AddDocument(Document<string, string> document)
    {
    }

    public Document<string, string>? RetrieveDocument(string collectionName, string documentId)
    {
        return null;
    }

    public Document<string, string>? RetrieveDocument(string documentId)
    {
        return null;
    }
}