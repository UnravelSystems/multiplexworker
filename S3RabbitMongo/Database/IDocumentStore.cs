using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Database.Mongo;

namespace S3RabbitMongo.Database;

public interface IDocumentStore
{
    public void AddDocument(Document document);
    public Document RetrieveDocument(string documentId);
}