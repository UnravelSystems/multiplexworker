namespace S3RabbitMongo.Job;

public interface IJobManager
{
    public long IncrementTask(String jobId);
    public long DecrementTask(String jobId);
    public bool RemoveTask(String jobId);
}