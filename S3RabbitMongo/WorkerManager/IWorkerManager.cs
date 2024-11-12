namespace S3RabbitMongo.MassTransit;

public interface IWorkerManager
{
    public void AddWorkItem(Message workItem);
    public void HandleWorkItem(Message workItem);
}