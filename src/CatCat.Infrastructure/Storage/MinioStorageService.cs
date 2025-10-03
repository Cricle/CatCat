using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace CatCat.Infrastructure.Storage;

/// <summary>
/// MinIO 对象存储服务实现
/// </summary>
public class MinioStorageService(
    IConfiguration configuration,
    ILogger<MinioStorageService> logger) : IStorageService
{
    private readonly string _endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
    private readonly string _accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
    private readonly string _secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
    private readonly string _bucketName = configuration["MinIO:BucketName"] ?? "catcat-media";
    private readonly bool _useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");
    private readonly IMinioClient _minioClient = CreateClient(configuration);

    private static IMinioClient CreateClient(IConfiguration configuration)
    {
        var endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? "minioadmin";
        var secretKey = configuration["MinIO:SecretKey"] ?? "minioadmin";
        var useSSL = bool.Parse(configuration["MinIO:UseSSL"] ?? "false");

        return new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSSL)
            .Build();
    }

    /// <summary>
    /// 初始化：确保 Bucket 存在
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var beArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);
            
            var exists = await _minioClient.BucketExistsAsync(beArgs, cancellationToken);
            
            if (!exists)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);
                await _minioClient.MakeBucketAsync(mbArgs, cancellationToken);
                
                logger.LogInformation("Created MinIO bucket: {BucketName}", _bucketName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize MinIO bucket: {BucketName}", _bucketName);
            throw;
        }
    }

    public async Task<string> UploadFileAsync(
        string fileName,
        Stream stream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // 生成唯一文件名（保留扩展名）
            var extension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            
            var putArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(uniqueFileName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType);
            
            await _minioClient.PutObjectAsync(putArgs, cancellationToken);
            
            logger.LogInformation("Uploaded file to MinIO: {FileName} -> {UniqueFileName}", fileName, uniqueFileName);
            
            // 返回文件标识（后续通过 GetFileUrlAsync 获取访问URL）
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload file to MinIO: {FileName}", fileName);
            throw;
        }
    }

    public async Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var rmArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);
            
            await _minioClient.RemoveObjectAsync(rmArgs, cancellationToken);
            
            logger.LogInformation("Deleted file from MinIO: {FileName}", fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file from MinIO: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            // 生成预签名URL（有效期7天）
            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName)
                .WithExpiry(60 * 60 * 24 * 7); // 7天
            
            var url = await _minioClient.PresignedGetObjectAsync(args);
            
            return url;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get file URL from MinIO: {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileName);
            
            await _minioClient.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

