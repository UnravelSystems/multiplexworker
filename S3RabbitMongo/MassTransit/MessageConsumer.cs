using MassTransit;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Job;

namespace S3RabbitMongo.MassTransit
{
    public class Message
    {
        public string RunId { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }
        public string ResultBucket { get; set; }
        public string MessageData { get; set; }
        public bool IsCreated { get; set; }

        public override string ToString()
        {
            return $"{RunId} {Bucket} {Key} {ResultBucket} {IsCreated} {MessageData}";
        }
    }
    
    public class MessageConsumer : IConsumer<Message>
    {
        readonly ILogger<MessageConsumer> _logger;
        readonly IBus _bus;
        readonly IJobManager _jobManager;

        public MessageConsumer(ILogger<MessageConsumer> logger, IBus bus, IJobManager jobManager)
        {
            _logger = logger;
            _bus = bus;
            _jobManager = jobManager;
        }
        
        public Task Consume(ConsumeContext<Message> context)
        {
            Message message = context.Message;
            string jobId = message.RunId;
            if (!message.IsCreated)
            {
                _jobManager.IncrementTask(jobId);
            }
            HandleMessage(message);
            
            long activeJobs = _jobManager.DecrementTask(jobId);
            if (activeJobs == 0)
            {
                _logger.LogInformation($"Job {jobId} has finished | removed={_jobManager.RemoveTask(message.RunId)}");
            }
            
            return Task.CompletedTask;
        }

        private void HandleMessage(Message message)
        {
            _logger.LogInformation("Message received: {@Message}", message);
            string messageData = message.MessageData;
            string[] parts = messageData.Split('/');
            
            if (parts.Length > 1)
            {
                _jobManager.IncrementTask(message.RunId);
                _bus.Publish(new Message()
                {
                    RunId = message.RunId,
                    Bucket = message.Bucket,
                    Key = message.Key,
                    ResultBucket = message.ResultBucket,
                    MessageData = String.Join("/", parts[..^1]),
                    IsCreated = true
                });
            }
            else if (Random.Shared.Next(0,10) == 1)
            {
                throw new Exception("Faulted");
            }
        }
    }
}