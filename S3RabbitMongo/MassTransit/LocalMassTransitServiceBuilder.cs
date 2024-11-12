using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S3RabbitMongo.Configuration.Interfaces;
using S3RabbitMongo.Job;
using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo.Configuration.ServiceBus;

[ServiceConfiguration(ServiceName = "mass_transit", ServiceType = "local")]
public class LocalMassTransitServiceBuilder : ExternalServiceBuilder
{
    public override void ConfigureServices(IServiceCollection serviceCollection, IConfigurationSection configuration)
    {
        serviceCollection.AddMassTransit(x =>
        {
            x.AddConsumer<MessageConsumer>();
            x.AddConsumer<FaultConsumer>();
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });
                       
        serviceCollection.AddHostedService<Producer>();
    }
}