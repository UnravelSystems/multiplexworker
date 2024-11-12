using MassTransit;
using Microsoft.Extensions.Hosting;

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
        var dict = await File.ReadAllTextAsync(@"D:\Development\multiplexworker\S3RabbitMongo\data.json", stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            await _bus.Publish(new Message()
            {
                RunId = Guid.NewGuid().ToString(),
                Bucket = "Bucket",
                Key = "Key",
                ResultBucket = "ResultBucket",
                MessageData = dict
            }, stoppingToken);
            
            await Task.Delay(1000, stoppingToken);
        }
    }
}