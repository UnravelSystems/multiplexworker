using S3RabbitMongo.MassTransit;
using S3RabbitMongo.Models;
using S3RabbitMongo.Models.Mongo;

namespace S3RabbitMongo.Database.Mongo;

public class TreeDocument : Document<StringTreeNode, Metadata>
{
    public TreeDocument(string jobId, string documentId, StringTreeNode data, Metadata metadata) : base(jobId,
        documentId, data, metadata)
    {
    }

    public TreeDocument(Document<StringTreeNode, Metadata> document) : base(document)
    {
    }
}