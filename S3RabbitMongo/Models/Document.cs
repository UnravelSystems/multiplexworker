namespace S3RabbitMongo.Models;

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