using System.Text.Json;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo.Worker;

[Worker(WorkerName = "NodeWorker")]
public class NodeWorker : IFileWorker<Message<Metadata, MessageData>>
{
    private readonly ILogger<NodeWorker> _logger;
    private IWorkerManager<Message<Metadata, MessageData>> _workerManager;
    public NodeWorker(ILogger<NodeWorker> logger)
    {
        _logger = logger;
    }

    public void SetWorkerManager(IWorkerManager<Message<Metadata, MessageData>> manager)
    {
        _workerManager = manager;
    }

    public bool Accepts(Message<Metadata, MessageData> request)
    {
        return true;
    }

    public void Process(Message<Metadata, MessageData> request)
    {
        var jsonConvert = request.Data.Root;
        foreach (TreeNode<string> child in jsonConvert.Children)
        {
            _workerManager.AddWorkItem(new Message<Metadata, MessageData>
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

    public void ProcessFile(Message<Metadata, MessageData> file)
    {
        throw new NotImplementedException();
    }
}