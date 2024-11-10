namespace S3RabbitMongo.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationOptionsAttribute: Attribute
{
    public string ServiceName { get; set; }
}