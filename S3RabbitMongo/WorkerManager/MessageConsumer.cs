using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Configuration.Datastore;
using S3RabbitMongo.Database;
using S3RabbitMongo.Job;

namespace S3RabbitMongo.MassTransit
{
    public class Message
    {
        public string? RunId { get; init; }
        public string? Bucket { get; init; }
        public string? Key { get; init; }
        public string? ResultBucket { get; init; }
        public string? MessageData { get; init; }
        public bool IsCreated { get; init; }

        public override string ToString()
        {
            return $"{RunId} {Bucket} {Key} {ResultBucket} {IsCreated} {MessageData}";
        }
    }
    
    public class MessageConsumer : IConsumer<Message>, IWorkerManager
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IBus _bus;
        private readonly IJobManager _jobManager;
        private readonly IDatastore _datastore;
        private readonly IDocumentStore _documentStore;

        public MessageConsumer(ILogger<MessageConsumer> logger, IBus bus, IJobManager jobManager, IDatastore datastore, IDocumentStore documentStore)
        {
            _logger = logger;
            _bus = bus;
            _jobManager = jobManager;
            _datastore = datastore;
            _documentStore = documentStore;
        }
        
        public Task Consume(ConsumeContext<Message> context)
        {
            Message message = context.Message;
            string jobId = message.RunId;
            if (!message.IsCreated)
            {
                _jobManager.AddTask(jobId, null);
            }
            HandleWorkItem(message);
            
            long activeJobs = _jobManager.RemoveTask(jobId, null);
            if (_jobManager.IsJobFinished(jobId))
            {
                _logger.LogInformation($"Job {jobId} has finished | removed={_jobManager.FinishJob(message.RunId)}");
            }
            
            return Task.CompletedTask;
        }

        public void HandleWorkItem(Message message)
        {
            _logger.LogInformation("Message received: {@Message}", message);
            string messageData = message.MessageData;
            var jsonConvert = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(messageData);
            foreach (string key in jsonConvert.Keys)
            {
                using (var memStream = new MemoryStream())
                using (var writer = new StreamWriter(memStream))
                {
                    writer.Write(messageData);
                    writer.Flush();
                    _documentStore.AddDocument(new Document(message.RunId, Guid.NewGuid().ToString(), messageData));
                    _datastore.StoreFile("test", $"{message.RunId}/{key}", memStream);
                }
                JsonElement val = jsonConvert[key];
                if (val.ValueKind == JsonValueKind.Object)
                {
                    AddWorkItem(new Message
                    {
                        RunId = message.RunId,
                        Bucket = message.Bucket,
                        Key = message.Key,
                        ResultBucket = message.ResultBucket,
                        MessageData = JsonSerializer.Serialize(val),
                        IsCreated = true
                    });
                }
                else
                {
                    _logger.LogInformation("Message received: {@Key}:{@Value}", key, val);
                }
            }
        }

        public void AddWorkItem(Message workItem)
        {
            _jobManager.AddTask(workItem.RunId, null);
            _bus.Publish(workItem);
        }
    }
}