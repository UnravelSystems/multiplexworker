using System.Text.Json;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.MassTransit;
using S3RabbitMongo.Models;

namespace S3RabbitMongo.Worker;

[Worker(WorkerName = "NodeWorker")]
public class NodeWorker : IFileWorker<WorkRequest<Metadata, MessageData>>
{
    private readonly ILogger<NodeWorker> _logger;
    private IWorkerManager<WorkRequest<Metadata, MessageData>> _workerManager;
    public NodeWorker(ILogger<NodeWorker> logger)
    {
        _logger = logger;
    }

    public void SetWorkerManager(IWorkerManager<WorkRequest<Metadata, MessageData>> manager)
    {
        _workerManager = manager;
    }

    public bool Accepts(WorkRequest<Metadata, MessageData> request)
    {
        return true;
    }

    public void Process(WorkRequest<Metadata, MessageData> request)
    {
        var jsonConvert = request.Data.Root;
        foreach (StringTreeNode child in jsonConvert.Children)
        {
            _workerManager.AddWorkItem(new WorkRequest<Metadata, MessageData>
            {
                JobId = request.JobId,
                Metadata = request.Metadata,
                Data = new MessageData()
                {
                    Root = child
                },
                IsCreated = true
            });
        }
    }

    public void ProcessFile(WorkRequest<Metadata, MessageData> file)
    {
        throw new NotImplementedException();
    }
}