﻿using Microsoft.Extensions.DependencyInjection;

namespace S3RabbitMongo.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public class ServiceConfigurationAttribute: Attribute
{
    public string ServiceType { get; set; } = "default";
    public string? ServiceName { get; set; }
    public ServiceLifetime Scope { get; set; } = ServiceLifetime.Singleton;
}