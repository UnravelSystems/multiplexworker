using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;

namespace S3RabbitMongo.Job
{
    public class Job
    {
        public string JobId { get; set; }
        public long CurrentTasks { get; set; }
        public DateTime StartTime { get; set; }
    }
    
    [ServiceConfiguration(ServiceType = "mongo", ServiceName = "job_manager", ServiceInterface = typeof(IJobManager))]
    public class MongoJobManager : IJobManager
    {
        private readonly ILogger<MongoJobManager> _logger;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Job> _collection;
        
        private readonly FindOneAndUpdateOptions<Job> _upsertOptions = new()
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        
        private readonly FindOneAndUpdateOptions<Job> _updateOptions = new()
        {
            IsUpsert = false,
            ReturnDocument = ReturnDocument.After
        };
        
        public MongoJobManager(IMongoClient client, IOptions<MongoOptions> options, ILogger<MongoJobManager> logger)
        {
            _database = client.GetDatabase(options.Value.Database);
            _logger = logger;
            _collection = _database.GetCollection<Job>("jobs");
        }

        public long IncrementTask(string jobId)
        {
            _logger.LogDebug($"Incrementing task {jobId}");
            Job foundJob = _collection.FindOneAndUpdate(
                Builders<Job>.Filter.Where(j => j.JobId == jobId), 
                Builders<Job>.Update
                    .SetOnInsert(j => j.StartTime, DateTime.UtcNow)
                    .SetOnInsert(j => j.JobId, jobId)
                    .SetOnInsert(j => j.CurrentTasks, 1)
                    .Inc("CurrentTasks", 1),
                _upsertOptions);
            return foundJob.CurrentTasks;
        }

        public long DecrementTask(string jobId)
        {
            _logger.LogDebug($"Decrementing task {jobId}");
            Job foundJob = _collection.FindOneAndUpdate(
                Builders<Job>.Filter.Where(j => j.JobId == jobId), 
                Builders<Job>.Update
                    .Inc("CurrentTasks", -1),
                _updateOptions);
            
            if (foundJob == null)
            {
                throw new Exception($"Job {jobId} not found");
            }
            return foundJob.CurrentTasks;
        }

        public bool RemoveTask(string jobId)
        {
            _logger.LogDebug($"Removing task {jobId}");
            DeleteResult result = _collection.DeleteOne(Builders<Job>.Filter.Where(j => j.JobId == jobId));
            return result.DeletedCount == 1;
        }
    }
}