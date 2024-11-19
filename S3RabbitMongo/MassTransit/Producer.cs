using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Hosting;
using S3RabbitMongo.Models;

namespace S3RabbitMongo.MassTransit;

public class Producer : BackgroundService
{
    private readonly IBus _bus;

    public Producer(IBus bus)
    {
        _bus = bus;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dict = await File.ReadAllTextAsync(@"data.json", stoppingToken);
        var data = JsonSerializer.Deserialize<StringTreeNode>(dict);
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = new WorkRequest<Metadata, MessageData>
                {
                JobId = Guid.NewGuid().ToString(),
                Metadata = new Metadata
                {
                    Bucket = "Bucket",
                    Key = "Key",
                    ResultBucket = "ResultBucket"
                },
                Data = new MessageData
                {
                    Root = data
                }
            };
            await _bus.Publish<WorkRequest<Metadata, MessageData>>(message, ctx =>
            {
                ctx.SetPriority(1);
            }, stoppingToken);
            
            await Task.Delay(100000000, stoppingToken);
        }
    }
}