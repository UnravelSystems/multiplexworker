using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;

namespace S3RabbitMongo.Database;

[ServiceConfiguration(ServiceName = "document_store", ServiceType = "local", ServiceInterface = typeof(IDocumentStore))]
public class LocalDocumentStore : IDocumentStore
{
    public void AddDocument(Document document)
    {
        
    }

    public Document RetrieveDocument(string documentId)
    {
        return null;
    }
}