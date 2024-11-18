using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo.Worker;

public interface IWorker<T>
{
    public void SetWorkerManager(IWorkerManager<T> manager);
    public bool Accepts(T request);
    public void Process(T request);
}