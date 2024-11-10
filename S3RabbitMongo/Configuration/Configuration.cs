using System.Reflection;

namespace S3RabbitMongo.Configuration;

public class Configuration
{
    private static Dictionary<string, Type> _configurationDiscriminators = new Dictionary<string, Type>();

    static Configuration()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in GetExternalServiceConfigurations(assembly))
            {
                ServiceConfigurationAttribute attribute = (ServiceConfigurationAttribute)type.GetCustomAttribute(typeof(ServiceConfigurationAttribute));
                _configurationDiscriminators.Add(attribute.ServiceName, type);
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

    public static void ParseConfiguration(string configurationPath)
    {
        
    }
}