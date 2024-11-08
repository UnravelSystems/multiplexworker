using MassTransit;
using Microsoft.Extensions.Logging;

namespace S3RabbitMongo.MassTransit
{
    public class Message
    {
        public string RunId { get; set; }
        public string Bucket { get; set; }
        public string Key { get; set; }
        public string ResultBucket { get; set; }
        public string MessageData { get; set; }

        public override string ToString()
        {
            return $"{RunId} {Bucket} {Key} {ResultBucket} {MessageData}";
        }
    }
    
    public class MessageConsumer : IConsumer<Message>
    {
        readonly ILogger<MessageConsumer> _logger;
        readonly IBus _bus;

        public MessageConsumer(ILogger<MessageConsumer> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }
        
        public Task Consume(ConsumeContext<Message> context)
        {
            Message message = context.Message;
            HandleMessage(message);
            
            return Task.CompletedTask;
        }

        private void HandleMessage(Message message)
        {
            _logger.LogInformation("Message received: {@Message}", message);

            string messageData = message.MessageData;
            string[] parts = messageData.Split('/');
            
            if (parts.Length > 1)
            {
                _bus.Publish(new Message()
                {
                    RunId = message.RunId,
                    Bucket = message.Bucket,
                    Key = message.Key,
                    ResultBucket = message.ResultBucket,
                    MessageData = String.Join("/", parts[..^1])
                });
            }
        }
    }
}