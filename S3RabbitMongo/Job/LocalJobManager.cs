using System.Collections.Concurrent;
using S3RabbitMongo.Configuration;

namespace S3RabbitMongo.Job;

[ServiceConfiguration(ServiceType = "local", ServiceName = "job_manager", ServiceInterface = typeof(IJobManager))]
public class LocalJobManager : IJobManager
{
    private ConcurrentDictionary<string, long> _jobs;
    public LocalJobManager()
    {
        _jobs = new ConcurrentDictionary<string, long>();
    }

    public long IncrementTask(String jobId)
    {
        return _jobs.AddOrUpdate(jobId, 1, (k, v) => v + 1);
    }

    public long DecrementTask(string jobId)
    {
        if (!_jobs.ContainsKey(jobId))
        {
            throw new Exception($"Job {jobId} not found");
        }
        return _jobs.AddOrUpdate(jobId, 1, (k, v) => v - 1);
    }

    public bool RemoveTask(string jobId)
    {
        return _jobs.TryRemove(jobId, out _);
    }
}