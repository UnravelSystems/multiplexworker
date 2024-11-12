namespace S3RabbitMongo.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public class OptionsConfigurationAttribute: Attribute
{
    public string ServiceName { get; set; }
}