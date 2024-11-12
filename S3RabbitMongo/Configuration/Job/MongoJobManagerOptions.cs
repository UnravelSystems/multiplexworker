using System.ComponentModel.DataAnnotations;

namespace S3RabbitMongo.Configuration.Job;

[OptionsConfiguration(ServiceName = "job_manager.mongo")]
public class MongoJobManagerOptions
{
    [Required]
    public string Collection { get; set; }
    
    [Required]
    public string Database { get; set; }
}