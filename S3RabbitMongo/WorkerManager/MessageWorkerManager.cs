using MassTransit;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.Database;
using S3RabbitMongo.Database.Mongo;
using S3RabbitMongo.Datastore;
using S3RabbitMongo.Job;
using S3RabbitMongo.Worker;

namespace S3RabbitMongo.MassTransit;

[ServiceConfiguration(ServiceName = "work_manager")]
public class MessageWorkerManager : IWorkerManager<Message>
{
    private readonly IBus _bus;
    private readonly ILogger<MessageWorkerManager> _logger;
    private readonly IEnumerable<IWorker<Message>> _workers;
    private readonly IDocumentStore<Document<string, string>> _documentStore;
    private readonly IDatastore _datastore;
    private readonly IJobManager _jobManager;
    
    public MessageWorkerManager(IBus bus, IEnumerable<IWorker<Message>> workers, IJobManager jobManager, IDatastore datastore, IDocumentStore<Document<string, string>> documentStore, ILogger<MessageWorkerManager> logger)
    {
        _bus = bus;
        _logger = logger;
        _workers = workers;
        _documentStore = documentStore;
        _jobManager = jobManager;
        _datastore = datastore;
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

    public void HandleWorkItem(Message workItem)
    {
        _logger.LogInformation("Message received: {@Message}", workItem);
        foreach (IWorker<Message> worker in _workers)
        {
            if (worker.Accepts(workItem))
            {
                worker.Process(workItem);
            }
        }
    }
}