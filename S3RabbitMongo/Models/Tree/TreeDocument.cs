using S3RabbitMongo.MassTransit;
using S3RabbitMongo.Models.S3;

namespace S3RabbitMongo.Models.Tree;

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