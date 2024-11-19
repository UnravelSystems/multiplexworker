using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Configuration.Datastore;
using S3RabbitMongo.Database;
using S3RabbitMongo.Database.Mongo;
using S3RabbitMongo.Datastore;
using S3RabbitMongo.Job;
using S3RabbitMongo.Models;
using S3RabbitMongo.Worker;

namespace S3RabbitMongo.MassTransit
{
    public class MessageData
    {
        public StringTreeNode? Root { get; set; }
    }

    public class RabbitWorkRequest : WorkRequest<Metadata, MessageData>
    {
        
    }

    public class MessageConsumer : IConsumer<WorkRequest<Metadata, MessageData>>,
        IWorkerManager<WorkRequest<Metadata, MessageData>>
    {
        private readonly ILogger<MessageConsumer> _logger;
        private readonly IBus _bus;
        private readonly IJobManager _jobManager;
        private readonly IDatastore _datastore;
        private readonly IDocumentStore<TreeDocument> _documentStore;
        private readonly IEnumerable<IWorker<WorkRequest<Metadata, MessageData>>> _workers;

        public MessageConsumer(ILogger<MessageConsumer> logger, IBus bus, IJobManager jobManager, IDatastore datastore,
            IDocumentStore<TreeDocument> documentStore,
            IEnumerable<IWorker<WorkRequest<Metadata, MessageData>>> workers)
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

        public Task Consume(ConsumeContext<WorkRequest<Metadata, MessageData>> context)
        {
            WorkRequest<Metadata, MessageData> workRequest = context.Message;
            string jobId = workRequest.JobId;
            if (!workRequest.IsCreated)
            {
                _jobManager.AddTask(jobId, null);
            }

            HandleWorkItem(workRequest);

            long activeJobs = _jobManager.RemoveTask(jobId, null);
            if (_jobManager.IsJobFinished(jobId))
            {
                _logger.LogInformation(
                    $"Job {jobId} has finished | removed={_jobManager.FinishJob(workRequest.JobId)} {Thread.CurrentThread.ManagedThreadId}");
            }

            return Task.CompletedTask;
        }

        public void HandleWorkItem(WorkRequest<Metadata, MessageData> workRequest)
        {
            _logger.LogInformation("Message received: {@Message}", workRequest);
            foreach (IWorker<WorkRequest<Metadata, MessageData>> worker in _workers)
            {
                if (worker.Accepts(workRequest))
                {
                    worker.Process(workRequest);
                }
            }
        }

        public void AddWorkItem(WorkRequest<Metadata, MessageData> workItem)
        {
            using (var memStream = new MemoryStream())
            using (var writer = new StreamWriter(memStream))
            {
                writer.Write(workItem.Data);
                writer.Flush();
                _documentStore.AddDocument(
                    new TreeDocument(
                        workItem.JobId,
                        Guid.NewGuid().ToString(),
                        workItem.Data.Root,
                        workItem.Metadata)
                );
                _datastore.StoreFile("test", $"{workItem.JobId}/{workItem.Metadata.Key}", memStream);
            }

            StringTreeNode? node = workItem.Data.Root;
            if (node != null && node.Children != null && node.Children.Any())
            {
                _jobManager.AddTask(workItem.JobId, null);
                _bus.Publish(workItem, ctx => { ctx.SetPriority(2); });
            }
        }
    }
}