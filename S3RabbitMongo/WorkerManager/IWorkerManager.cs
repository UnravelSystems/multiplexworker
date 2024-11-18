namespace S3RabbitMongo.MassTransit;

public interface IWorkerManager<T>
{
    public void AddWorkItem(T workItem);
    public void HandleWorkItem(T workItem);
}