using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace S3RabbitMongo.Configuration.Datastore;

[ServiceConfiguration(ServiceType = "S3", ServiceName = "datastore", ServiceInterface = typeof(IDatastore))]
public class S3Datastore : IDatastore
{
    private readonly ILogger<S3Datastore> _logger;
    private readonly IAmazonS3 _amazonS3;
    public S3Datastore(ILogger<S3Datastore> logger, IAmazonS3 amazonS3)
    {
        _logger = logger;
        _amazonS3 = amazonS3;
    }
    
    public void StoreFile(string bucket, string key, Stream inStream)
    {
        var fileTransferUtility =
            new TransferUtility(_amazonS3);
        TransferUtilityUploadRequest req = new TransferUtilityUploadRequest()
        {
            AutoCloseStream = false,
            BucketName = bucket,
            Key = key,
            InputStream = inStream
        };
        
        fileTransferUtility.Upload(req);
    }

    public void StoreFile(string bucket, string key, string inFile)
    {
        using (var fileStream = File.OpenRead(inFile))
        {
            StoreFile(bucket, key, fileStream);
        }
    }

    public Stream GetFile(string bucket, string key)
    {
        throw new NotImplementedException();
    }

    public void GetFile(string bucket, string key, string outFile)
    {
        throw new NotImplementedException();
    }
}