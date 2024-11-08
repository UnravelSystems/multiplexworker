using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo;

class Program
{
    public static void Main(string[] args)
    {
        ConfigureMassTransit(args).Build().Run();
    }

    private static IHostBuilder ConfigureMassTransit(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                   .ConfigureServices(services =>
                   {
                       services.AddMassTransit(x =>
                       {
                           x.AddConsumer<MessageConsumer>();
                           x.UsingInMemory((context, cfg) =>
                           {
                               cfg.ConfigureEndpoints(context);
                           });
                       });
                       services.AddHostedService<Producer>();
                   });
    }
}