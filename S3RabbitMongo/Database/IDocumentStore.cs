using S3RabbitMongo.Configuration.Database.External;

namespace S3RabbitMongo.Database;

public interface IDocumentStore
{
    public void AddDocument(Document document);
    public Document RetrieveDocument(string documentId);
}