using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Configuration.Datastore;
using S3RabbitMongo.Database;
using S3RabbitMongo.Database.Mongo;
using S3RabbitMongo.Datastore;
using S3RabbitMongo.Job;
using S3RabbitMongo.Worker;

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
    
    public class MessageConsumer : IConsumer<Message>, IWorkerManager<Message>
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IBus _bus;
        private readonly IJobManager _jobManager;
        private readonly IDatastore _datastore;
        private readonly IDocumentStore<Document<string, string>> _documentStore;
        private readonly IEnumerable<IWorker<Message>> _workers;

        public MessageConsumer(ILogger<MessageConsumer> logger, IBus bus, IJobManager jobManager, IDatastore datastore, IDocumentStore<Document<string, string>> documentStore, IEnumerable<IWorker<Message>> workers)
        {
            _logger = logger;
            _bus = bus;
            _jobManager = jobManager;
            _datastore = datastore;
            _documentStore = documentStore;
            _workers = workers;
            
            // TODO decouple things so that there is no circular dependency
            foreach (var worker in workers)
            {
                worker.SetWorkerManager(this);
            }
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
                _logger.LogInformation($"Job {jobId} has finished | removed={_jobManager.FinishJob(message.RunId)} {Thread.CurrentThread.ManagedThreadId}");
            }
            
            return Task.CompletedTask;
        }

        public void HandleWorkItem(Message message)
        {
            _logger.LogInformation("Message received: {@Message}", message);
            foreach (IWorker<Message> worker in _workers)
            {
                if (worker.Accepts(message))
                {
                    worker.Process(message);
                }
            }
        }

        public void AddWorkItem(Message workItem)
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            {
                writer.Write(workItem.MessageData);
                writer.Flush();
                _documentStore.AddDocument(new Document<string, string>(workItem.RunId, Guid.NewGuid().ToString(), workItem.MessageData, null));
                _datastore.StoreFile("test", $"{workItem.RunId}/{workItem.Key}", memStream);
            }
            
            _jobManager.AddTask(workItem.RunId, null);
            _bus.Publish(workItem, ctx =>
            {
                ctx.SetPriority(2);
            });
        }
    }
}