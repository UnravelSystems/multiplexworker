using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S3RabbitMongo.Configuration.Interfaces;

namespace S3RabbitMongo.Configuration;

public static class RegisterServiceFromConfigurationExtension
{
    private static Dictionary<string, Dictionary<string, Type>> _serviceMap = new Dictionary<string, Dictionary<string, Type>>();

    static RegisterServiceFromConfigurationExtension()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in GetExternalServiceConfigurations(assembly))
            {
                ServiceConfigurationAttribute attribute = (ServiceConfigurationAttribute)type.GetCustomAttribute(typeof(ServiceConfigurationAttribute));
                if (!_serviceMap.ContainsKey(attribute.ServiceName))
                {
                    _serviceMap.Add(attribute.ServiceName, new Dictionary<string, Type>());
                }
                
                _serviceMap[attribute.ServiceName].Add(attribute.ServiceType, type);
            }
        }
    }
    
    static IEnumerable<Type> GetExternalServiceConfigurations(Assembly assembly) {
        foreach(Type type in assembly.GetTypes()) {
            if (type.GetCustomAttributes(typeof(ServiceConfigurationAttribute), true).Length > 0) {
                yield return type;
            }
        }
    }

    static Type? GetInterfaceType(Type serviceType)
    {
        ServiceConfigurationAttribute attribute = (ServiceConfigurationAttribute)serviceType.GetCustomAttribute(typeof(ServiceConfigurationAttribute));
        return attribute.ServiceInterface;
    }
    
    public static IServiceCollection RegisterServicesFromConfiguration(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        IConfigurationSection servicesSection = configuration.GetSection("services");
        
        HashSet<string> seenServices = new();
        foreach (IConfigurationSection serviceSection in servicesSection.GetChildren())
        {
            string serviceName = serviceSection.GetValue<string>("serviceName")!;
            if (!_serviceMap.TryGetValue(serviceName, out Dictionary<string, Type> serviceTypeMap))
            {
                throw new InvalidOperationException($"No service class found for '{serviceName}'.");
            }
            
            string serviceTypeName = serviceSection.GetValue<string>("serviceType", "default")!;
            if (!serviceTypeMap.TryGetValue(serviceTypeName, out Type serviceType))
            {
                throw new InvalidOperationException($"No service type found for '{serviceName}/{serviceTypeName}'.");
            }
            
            IConfigurationSection dependsOn = serviceSection.GetSection("dependsOn");
            if (dependsOn.Exists())
            {
                foreach (IConfigurationSection dependsOnSection in dependsOn.GetChildren())
                {
                    string? dependsOnServiceName = dependsOnSection.GetValue<string>("serviceName");
                    if (!string.IsNullOrEmpty(dependsOnServiceName) && !seenServices.Contains(dependsOnServiceName))
                    {
                        throw new ConfigurationException($"Service '{serviceName}' depends on '{dependsOnServiceName}'.");
                    }
                }
            }
            
            seenServices.Add(serviceName);

            try
            {
                // More complex type of service builder, call the appropriate method for configuring the service
                if (typeof(ExternalServiceBuilder).IsAssignableFrom(serviceType))
                {
                    ExternalServiceBuilder? instance = (ExternalServiceBuilder)Activator.CreateInstance(serviceType)!;
                    instance.ConfigureServices(serviceCollection, serviceSection);
                }
                else
                {
                    // Simple service
                    Type? interfaceType = GetInterfaceType(serviceType);
                    if (interfaceType != null)
                    {
                        serviceCollection.AddSingleton(interfaceType, serviceType);
                    }
                    else
                    {
                        serviceCollection.AddSingleton(serviceType);
                    }
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"The service '{serviceName}' is not registered.", e);
            }
        }

        return serviceCollection;
    }
}