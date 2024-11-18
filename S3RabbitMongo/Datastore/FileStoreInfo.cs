namespace S3RabbitMongo.Configuration.Datastore;

public class FileStoreInfo
{
    public long Size { get; set; }
    public long Offset { get; set; }
    public string? Path { get; set; }
    public string? MD5 { get; set; }
}