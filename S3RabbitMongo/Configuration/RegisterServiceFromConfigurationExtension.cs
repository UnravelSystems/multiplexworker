using System.Reflection;
using MassTransit;
using MassTransit.Configuration;
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
        ServiceConfigurationAttribute attribute = (ServiceConfigurationAttribute)serviceType.GetCustomAttribute(typeof(ServiceConfigurationAttribute))!;
        return attribute.ServiceInterface;
    }

    static string? GetServiceScope(Type serviceType)
    {
        ServiceConfigurationAttribute attribute = (ServiceConfigurationAttribute)serviceType.GetCustomAttribute(typeof(ServiceConfigurationAttribute))!;
        return attribute.Scope;
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
            string serviceTypeName = serviceSection.GetValue<string>("serviceType", "default")!;
            Type serviceType = ValidateAndGetServiceType(serviceName, serviceTypeName);
            
            ValidateDependsOn(serviceSection, seenServices, serviceName);

            try
            {
                IConfigurationSection optionsSection = serviceSection.GetSection("options");
                if (optionsSection.Exists())
                {
                    RegisterOptionsFromConfigurationExtension.AddOptionsWithValidateOnStart(serviceCollection, optionsSection, $"{serviceName}.{serviceTypeName}");
                }
                
                // More complex type of service builder, call the appropriate method for configuring the services
                if (typeof(ExternalServiceBuilder).IsAssignableFrom(serviceType))
                {
                    ExternalServiceBuilder instance = (ExternalServiceBuilder)Activator.CreateInstance(serviceType)!;
                    instance.ConfigureServices(serviceCollection, serviceSection);
                }
                else
                {
                    // Simple service
                    Type? interfaceType = GetInterfaceType(serviceType);
                    RegisterService(serviceCollection, serviceType, interfaceType, GetServiceScope(serviceType));
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Failed to register service: '{serviceName}'", e);
            }
            
            seenServices.Add(serviceName.ToLower());
        }

        return serviceCollection;
    }

    private static void RegisterService(IServiceCollection serviceCollection, Type serviceType, Type? interfaceType, string? scope)
    {
        if (interfaceType == null)
        {
            interfaceType = serviceType;
        }

        switch (scope)
        {
            case "singleton":
                serviceCollection.AddSingleton(interfaceType, serviceType);
                break;
            case "scoped":
                serviceCollection.AddScoped(interfaceType, serviceType);
                break;
            case "transient":
                serviceCollection.AddTransient(interfaceType, serviceType);
                break;
            default:
                throw new InvalidOperationException($"The service-scope '{scope}' is not supported.");
        }
    }

    private static Type ValidateAndGetServiceType(string serviceName, string serviceTypeName)
    {
        if (!_serviceMap.TryGetValue(serviceName, out Dictionary<string, Type> serviceTypeMap))
        {
            throw new InvalidOperationException($"No service class found for '{serviceName}'.");
        }
            
        if (!serviceTypeMap.TryGetValue(serviceTypeName, out Type serviceType))
        {
            throw new InvalidOperationException($"No service type found for '{serviceName}/{serviceTypeName}'.");
        }

        return serviceType;
    }

    private static void ValidateDependsOn(IConfiguration serviceConfiguration, HashSet<string> seenServices, String serviceName)
    {
            
        IConfigurationSection dependsOn = serviceConfiguration.GetSection("dependsOn");
        if (dependsOn.Exists())
        {
            foreach (IConfigurationSection dependsOnSection in dependsOn.GetChildren())
            {
                string? dependsOnServiceName = dependsOnSection.Value;
                if (!string.IsNullOrEmpty(dependsOnServiceName) && !seenServices.Contains(dependsOnServiceName.ToLower()))
                {
                    throw new ConfigurationException($"Missing Dependency: Service '{serviceName}' depends on '{dependsOnServiceName}'.");
                }
            }
        }
    }
}