﻿using MassTransit.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using S3RabbitMongo.Configuration;
using S3RabbitMongo.Configuration.Database.External;
using S3RabbitMongo.Configuration.Job;

namespace S3RabbitMongo.Job
{
    public class Job
    {
        [BsonId]
        public ObjectId Id { get; set; }
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
        
        public MongoJobManager(IMongoClient client, IOptions<MongoJobManagerOptions> options, ILogger<MongoJobManager> logger)
        {
            _database = client.GetDatabase(options.Value.Database);
            _logger = logger;
            CreateCollectionIfNotExists(options.Value.Collection);
            _collection = _database.GetCollection<Job>(options.Value.Collection);
        }

        private void CreateCollectionIfNotExists(string collectionName)
        {
            _database.CreateCollection(collectionName);
        }
        
        public long AddTask(string jobId, string taskId)
        {
            Job foundJob = _collection.FindOneAndUpdate(
                Builders<Job>.Filter.Where(j => j.JobId == jobId), 
                Builders<Job>.Update
                    .SetOnInsert(j => j.StartTime, DateTime.UtcNow)
                    .SetOnInsert(j => j.JobId, jobId)
                    .Inc("CurrentTasks", 1),
                _upsertOptions);
            _logger.LogInformation($"Incrementing task {jobId} | current_tasks: {foundJob.CurrentTasks}");
            return foundJob.CurrentTasks;
        }

        public long RemoveTask(string jobId, string taskId)
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
            _logger.LogInformation($"Decrementing task {jobId} | current_tasks: {foundJob.CurrentTasks}");
            return foundJob.CurrentTasks;
        }

        public bool IsJobFinished(string jobId)
        {
            Job job = _collection.FindSync(Builders<Job>.Filter.Where(j => j.JobId == jobId)).First();
            if (job == null)
            {
                throw new Exception($"Job {jobId} not found");
            }
            
            return job.CurrentTasks == 0;
        }

        public bool FinishJob(string jobId)
        {
            _logger.LogInformation($"Removing task {jobId}");
            DeleteResult result = _collection.DeleteOne(Builders<Job>.Filter.Where(j => j.JobId == jobId));
            return result.DeletedCount == 1;
        }
    }
}