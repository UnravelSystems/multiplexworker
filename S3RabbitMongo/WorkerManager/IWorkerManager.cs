namespace S3RabbitMongo.MassTransit;

public interface IWorkerManager {}

public interface IWorkerManager<T> : IWorkerManager
{
    public void AddWorkItem(T workItem);
    public void HandleWorkItem(T workItem);
}