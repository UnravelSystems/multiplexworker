namespace S3RabbitMongo.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceConfigurationAttribute: Attribute
{
    public string ServiceType { get; set; } = "default";
    public string? ServiceName { get; set; }

    public Type? ServiceInterface { get; set; }
    public string? Scope { get; set; } = "singleton";
}