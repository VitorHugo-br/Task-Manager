using Minio;
using Minio.DataModel.Args;

namespace Task_Manager.Services;

public class MinIoStorageService(IMinioClient minio)
{

    public async Task UploadFileAsync(string bucketName, string objectName, Stream contentStream, string contentType)
    {
        bool found = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found) await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(contentStream)
            .WithObjectSize(contentStream.Length)
            .WithContentType(contentType);

        await minio.PutObjectAsync(putObjectArgs);
    }

    public async Task<Object> GetFilesAsync(string bucketName, string objectName)
    {
        bool found = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found) return new { msg = "bucket nao encontrado" };

        var files = await minio.GetObjectAsync(new GetObjectArgs().WithBucket(bucketName).WithObject(objectName));

        return files;
        
    }
}
