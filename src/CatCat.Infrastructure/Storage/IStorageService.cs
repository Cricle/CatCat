namespace CatCat.Infrastructure.Storage;

/// <summary>
/// 对象存储服务接口（图片、视频等）
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="stream">文件流</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件URL</returns>
    Task<string> UploadFileAsync(string fileName, Stream stream, string contentType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteFileAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 获取文件URL（预签名URL，有效期7天）
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件访问URL</returns>
    Task<string> GetFileUrlAsync(string fileName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
}

