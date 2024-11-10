using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using S3RabbitMongo.Configuration.Interfaces;

namespace S3RabbitMongo.Configuration;

public static class RegisterOptionsFromConfigurationExtension
{
    private static Dictionary<string, Type> _optionsMapping = new Dictionary<string, Type>();

    static RegisterOptionsFromConfigurationExtension()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in GetExternalServiceConfigurations(assembly))
            {
                ConfigurationOptionsAttribute attribute = (ConfigurationOptionsAttribute)type.GetCustomAttribute(typeof(ConfigurationOptionsAttribute));
                _optionsMapping.Add(attribute.ServiceName, type);
            }
        }
    }
    
    static IEnumerable<Type> GetExternalServiceConfigurations(Assembly assembly) {
        foreach(Type type in assembly.GetTypes()) {
            if (type.GetCustomAttributes(typeof(ConfigurationOptionsAttribute), true).Length > 0) {
                yield return type;
            }
        }
    }
    
    public static IServiceCollection RegisterOptionsFromConfiguration(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        IConfigurationSection optionsSection = configuration.GetSection("options");
        
        foreach (IConfigurationSection optionSection in optionsSection.GetChildren())
        {
            string optionTypeName = optionSection.Key;
            if (!_optionsMapping.TryGetValue(optionTypeName, out Type optionType))
            {
                throw new InvalidOperationException($"The service '{optionTypeName}' is not registered.");
            }
            
            try
            {
                // Because Configure is a generic function and we need to register this as the specific generic 
                // option type saved from the attributes we need to use reflection to register the configuration
                typeof(OptionsConfigurationServiceCollectionExtensions)
                    .GetMethod("Configure", [typeof(IServiceCollection), typeof(IConfiguration)])
                    ?.MakeGenericMethod(optionType)
                    .Invoke(serviceCollection, [serviceCollection, optionSection]);
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"The service '{optionTypeName}' is not registered.", ex);
            }
        }

        return serviceCollection;
    }
}