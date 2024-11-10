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
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new Message()
            {
                RunId = Guid.NewGuid().ToString(),
                Bucket = "Bucket",
                Key = "Key",
                ResultBucket = "ResultBucket",
                MessageData = "1/2/3/4/5"
            });
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}