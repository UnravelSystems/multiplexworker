using MassTransit;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Job;

namespace S3RabbitMongo.MassTransit;

public class FaultConsumer : IConsumer<Fault<Message<Metadata, MessageData>>>
{
    readonly ILogger<MessageConsumer> _logger;
    readonly IJobManager _jobManager;

    public FaultConsumer(ILogger<MessageConsumer> logger, IJobManager jobManager)
    {
        _logger = logger;
        _jobManager = jobManager;
    }
    
    public async Task Consume(ConsumeContext<Fault<Message<Metadata, MessageData>>> context)
    {
        _logger.LogInformation("Fault consumer received: {@context}", context);
        Message<Metadata, MessageData> message = context.Message.Message;
        long activeJobs = _jobManager.RemoveTask(message.JobId, null);
        if (_jobManager.IsJobFinished(message.JobId))
        {
            _logger.LogInformation($"Job {message.JobId} has finished through fault: {_jobManager.FinishJob(message.JobId)}");
        }
    }
}