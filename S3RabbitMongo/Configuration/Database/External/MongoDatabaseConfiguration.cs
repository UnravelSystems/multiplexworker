using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using S3RabbitMongo.Configuration.Interfaces;

namespace S3RabbitMongo.Configuration.Database.External;

[ServiceConfiguration(ServiceName = "mongo_old")]
public class MongoDatabaseConfiguration : ExternalServiceBuilder
{
    public override void ConfigureServices(IServiceCollection services, IConfigurationSection configuration)
    {
        string? uri = configuration.GetValue<string>("uri");
        int port = configuration.GetValue("port", 27017);
        string? username = configuration.GetValue<string>("username");
        string? password = configuration.GetValue<string>("password");
        string? database = configuration.GetValue<string>("database");

        if (string.IsNullOrEmpty(uri))
        {
            throw new ArgumentNullException(nameof(uri));
        }
        if (string.IsNullOrEmpty(username))
        {
            throw new ArgumentNullException(nameof(username));
        }
        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentNullException(nameof(password));
        }
        if (string.IsNullOrEmpty(database))
        {
            throw new ArgumentNullException(nameof(database));
        }
        
        MongoClientSettings settings = new()
        {
            Scheme = ConnectionStringScheme.MongoDB,
            Server = new MongoServerAddress(uri, port),
            Credential = MongoCredential.CreateCredential(database, username, password)
        };
        MongoClient mongoClient = new(settings);
        
        services.AddSingleton<IMongoClient>(_ => mongoClient);
        services.AddSingleton<IMongoDatabase>(_ => mongoClient.GetDatabase(database));
    }
}