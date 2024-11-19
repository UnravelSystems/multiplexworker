namespace S3RabbitMongo.MassTransit;

public class WorkRequest<T1, T2>
{
    public string? JobId { get; init; }
    public T1 Metadata { get; init; }
    public T2 Data { get; init; }
    public bool IsCreated { get; init; }

    public override string ToString()
    {
        return $"{JobId} {Metadata} {IsCreated} {Data}";
    }
}