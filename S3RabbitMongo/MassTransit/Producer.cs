using MassTransit;
using Microsoft.Extensions.Hosting;

namespace S3RabbitMongo.MassTransit;

public class Producer : BackgroundService
{
    readonly IBus _bus;

    public Producer(IBus bus)
    {
        _bus = bus;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var dict = File.ReadAllText("D:\\Development\\multiplexworker\\S3RabbitMongo\\data.json");
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new Message()
            {
                RunId = Guid.NewGuid().ToString(),
                Bucket = "Bucket",
                Key = "Key",
                ResultBucket = "ResultBucket",
                MessageData = dict
            });
            
            await Task.Delay(100000000, stoppingToken);
        }
    }
}