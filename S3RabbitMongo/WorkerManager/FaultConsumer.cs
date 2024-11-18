using MassTransit;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Job;

namespace S3RabbitMongo.MassTransit;

public class FaultConsumer : IConsumer<Fault<Message>>
{
    readonly ILogger<MessageConsumer> _logger;
    readonly IJobManager _jobManager;

    public FaultConsumer(ILogger<MessageConsumer> logger, IJobManager jobManager)
    {
        _logger = logger;
        _jobManager = jobManager;
    }
    
    public async Task Consume(ConsumeContext<Fault<Message>> context)
    {
        _logger.LogInformation("Fault consumer received: {@context}", context);
        Message message = context.Message.Message;
        long activeJobs = _jobManager.RemoveTask(message.RunId, null);
        if (_jobManager.IsJobFinished(message.RunId))
        {
            _logger.LogInformation($"Job {message.RunId} has finished through fault: {_jobManager.FinishJob(message.RunId)}");
        }
    }
}