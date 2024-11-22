﻿using MongoDB.Bson;
using S3RabbitMongo.Database.Mongo;
using S3RabbitMongo.MassTransit;
using S3RabbitMongo.Models.S3;
using S3RabbitMongo.Models.Tree;

namespace S3RabbitMongo.Models.Mongo;

public class MongoDocument : TreeDocument
{
    public MongoDocument(string jobId, string documentId, StringTreeNode data, Metadata metadata) : base(jobId, documentId, data, metadata)
    {
    }

    public MongoDocument(Document<StringTreeNode, Metadata> document) : base(document)
    {
    }

    public ObjectId Id { get; set; }
}