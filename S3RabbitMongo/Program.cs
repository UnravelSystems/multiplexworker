using CommandLine;
using Microsoft.Extensions.Hosting;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Job;
using S3RabbitMongo.MassTransit;

namespace S3RabbitMongo;

class Program
{
    public static void Main(string[] args)
    {
        Configure(args).Build().Run();
    }

    public static IHostBuilder Configure(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, builder) => builder.AddJsonFile("D:\\Development\\multiplexworker\\S3RabbitMongo\\configuration.json"))
            .ConfigureServices((ctx, services) =>
            {
                services.RegisterOptionsFromConfiguration(ctx.Configuration);
                services.RegisterServicesFromConfiguration(ctx.Configuration);
            });
    }
}