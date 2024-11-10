using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace S3RabbitMongo.Configuration.Interfaces;

public abstract class ExternalServiceBuilder
{
    public abstract void ConfigureServices(IServiceCollection serviceCollection, IConfigurationSection configuration);
}