using System.Text.Json;
using System.Text.Json.Serialization;
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
    public class Metadata
    {
        public string? Bucket { get; init; }
        public string? Key { get; init; }
        public string? ResultBucket { get; init; }
        public string? ResultPrefix { get; init; }
    }

    public class TreeNode<T>
    {
        [JsonPropertyName("value")] 
        public T Value { get; set; }
        [JsonPropertyName("children")] 
        public List<TreeNode<T>>? Children { get; set; }

        public TreeNode(T value)
        {
            Value = value;
        }

        public void AddChild(TreeNode<T> child)
        {
            if (Children == null)
            {
                Children = new List<TreeNode<T>>();
            }
            Children.Add(child);
        }
    }

    public class MessageData
    {
        public TreeNode<string>? Root { get; set; }
    }

    public class Message<T1, T2>
    {
        public string? JobId { get; init; }
        public T1 Metadata { get; init; }
        public T2 Data { get; init; }
        public bool IsCreated { get; init; }

        public override string ToString()
        {
            return $"{JobId} {Metadata} {IsCreated} {Data}";
        }
    }

    public class MessageConsumer : IConsumer<Message<Metadata, MessageData>>,
        IWorkerManager<Message<Metadata, MessageData>>
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IBus _bus;
        private readonly IJobManager _jobManager;
        private readonly IDatastore _datastore;
        private readonly IDocumentStore<Document<TreeNode<string>, string>> _documentStore;
        private readonly IEnumerable<IWorker<Message<Metadata, MessageData>>> _workers;

        public MessageConsumer(ILogger<MessageConsumer> logger, IBus bus, IJobManager jobManager, IDatastore datastore,
            IDocumentStore<Document<TreeNode<string>, string>> documentStore,
            IEnumerable<IWorker<Message<Metadata, MessageData>>> workers)
        {
            _logger = logger;
            _bus = bus;
            _jobManager = jobManager;
            _datastore = datastore;
            _documentStore = documentStore;
            _workers = workers;

            // TODO decouple things so that there is no circular dependency
            foreach (var worker in _workers)
            {
                worker.SetWorkerManager(this);
            }
        }

        public Task Consume(ConsumeContext<Message<Metadata, MessageData>> context)
        {
            Message<Metadata, MessageData> message = context.Message;
            string jobId = message.JobId;
            if (!message.IsCreated)
            {
                _jobManager.AddTask(jobId, null);
            }

            HandleWorkItem(message);

            long activeJobs = _jobManager.RemoveTask(jobId, null);
            if (_jobManager.IsJobFinished(jobId))
            {
                _logger.LogInformation(
                    $"Job {jobId} has finished | removed={_jobManager.FinishJob(message.JobId)} {Thread.CurrentThread.ManagedThreadId}");
            }

            return Task.CompletedTask;
        }

        public void HandleWorkItem(Message<Metadata, MessageData> message)
        {
            _logger.LogInformation("Message received: {@Message}", message);
            foreach (IWorker<Message<Metadata, MessageData>> worker in _workers)
            {
                if (worker.Accepts(message))
                {
                    worker.Process(message);
                }
            }
        }

        public void AddWorkItem(Message<Metadata, MessageData> workItem)
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            {
                writer.Write(workItem.Data);
                writer.Flush();
                _documentStore.AddDocument(
                    new Document<TreeNode<string>, string>(
                        workItem.JobId, 
                        Guid.NewGuid().ToString(),
                        workItem.Data.Root, 
                        null)
                    );
                _datastore.StoreFile("test", $"{workItem.JobId}/{workItem.Metadata.Key}", memStream);
            }
            
            TreeNode<string>? node = workItem.Data.Root;
            if (node != null && node.Children != null && node.Children.Any())
            {
                _jobManager.AddTask(workItem.JobId, null);
                _bus.Publish(workItem, ctx => { ctx.SetPriority(2); });
            }
        }
    }
}