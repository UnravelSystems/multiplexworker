using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace S3RabbitMongo.Configuration.Database.External;

[ServiceConfiguration(ServiceName = "mongo", ServiceInterface = typeof(IMongoClient))]
public class MongoClientWrapper : MongoClient
{
    public MongoClientWrapper(IOptions<MongoOptions> options) : base(options.Value.ClientSettings)
    {
    }
}