namespace S3RabbitMongo.Models.S3;

public class Metadata
{
    public string? Bucket { get; init; }
    public string? Key { get; init; }
    public string? ResultBucket { get; init; }
    public string? ResultPrefix { get; init; }
}