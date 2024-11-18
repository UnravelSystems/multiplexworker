using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo.Worker;

public interface IFileWorker<T> : IWorker<T>
{
    public void ProcessFile(T file);
}