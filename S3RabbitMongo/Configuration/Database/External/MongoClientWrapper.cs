using Amazon.SecurityToken.Model;
using MassTransit.Middleware;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using S3RabbitMongo.Configuration.Interfaces;

namespace S3RabbitMongo.Configuration.Database.External
{
    [ConfigurationOptions(ServiceName = "mongo")]
    public class MongoOptions
    {
        public string URI { get; set; }
        public int Port { get; set; } = 27017;
        public string Database { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }

        public MongoCredential Credentials => MongoCredential.CreateCredential(Database, Username, Password);

        public MongoClientSettings ClientSettings
        {
            get => new MongoClientSettings()
            {
                Scheme = ConnectionStringScheme.MongoDB,
                Server = new MongoServerAddress(URI, Port),
                Credential = Credentials
            };
        }
    }

    [ServiceConfiguration(ServiceName = "mongo", ServiceInterface = typeof(IMongoClient))]
    public class MongoClientWrapper : MongoClient
    {
        public MongoClientWrapper(IOptions<MongoOptions> options) : base(options.Value.ClientSettings)
        {
        }
    }
    
}