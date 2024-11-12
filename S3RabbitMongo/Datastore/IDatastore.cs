namespace S3RabbitMongo.Configuration.Datastore;

public interface IDatastore
{
    public void StoreFile(string storeArea, string storagePath, Stream dataStream);
    public void StoreFile(string storeArea, string storagePath, string inFile);
    public Stream GetFile(string storeArea, string storagePath);
    public void GetFile(string storeArea, string storagePath, string outFile);
    
}