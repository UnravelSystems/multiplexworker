using System.Text.Json;
using Microsoft.Extensions.Logging;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo.Worker;

[Worker(WorkerName = "NodeWorker")]
public class NodeWorker : IFileWorker<Message>
{
    private readonly ILogger<NodeWorker> _logger;
    private IWorkerManager<Message> _workerManager;
    public NodeWorker(ILogger<NodeWorker> logger)
    {
        _logger = logger;
    }

    public void SetWorkerManager(IWorkerManager<Message> manager)
    {
        this._workerManager = manager;
    }

    public bool Accepts(Message request)
    {
        return true;
    }

    public void Process(Message request)
    {
        var jsonConvert = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.MessageData);
        foreach (string key in jsonConvert.Keys)
        {
            JsonElement val = jsonConvert[key];
            if (val.ValueKind == JsonValueKind.Object)
            {
                _workerManager.AddWorkItem(new Message
                {
                    RunId = request.RunId,
                    Bucket = request.Bucket,
                    Key = request.Key,
                    ResultBucket = request.ResultBucket,
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

    public void ProcessFile(Message file)
    {
        throw new NotImplementedException();
    }
}