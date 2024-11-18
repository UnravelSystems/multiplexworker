using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Database.Mongo;

namespace S3RabbitMongo.Database;

public interface IDocumentStore<T>
{
    public void AddDocument(string collectionName, T document);
    public void AddDocument(T document);
    public T? RetrieveDocument(string collectionName, string documentId);
    public T? RetrieveDocument(string documentId);
}