using System.Security.Claims;
using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Storage;
using Microsoft.AspNetCore.Mvc;

namespace CatCat.API.Endpoints;

public static class StorageEndpoints
{
    public static void MapStorageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/storage")
            .RequireAuthorization()
            .WithTags("Storage")
            .WithOpenApi();

        group.MapPost("upload", UploadFile)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>(200)
            .Produces<ApiResult>(400)
            .Produces(401);

        group.MapDelete("{fileName}", DeleteFile)
            .Produces(200)
            .Produces<ApiResult>(404)
            .Produces(401);

        group.MapGet("{fileName}/url", GetFileUrl)
            .Produces<FileUrlResponse>(200)
            .Produces<ApiResult>(404)
            .Produces(401);
    }

    private static async Task<IResult> UploadFile(
        HttpRequest request,
        ClaimsPrincipal user,
        [FromServices] IStorageService storageService,
        [FromServices] ILogger<IStorageService> logger)
    {
        if (!user.TryGetUserId(out var userId))
            return Results.Unauthorized();

        // 检查是否有文件
        if (!request.HasFormContentType || request.Form.Files.Count == 0)
            return Results.BadRequest(ApiResult.Fail("No file uploaded"));

        var file = request.Form.Files[0];

        // 验证文件大小（最大 50MB）
        if (file.Length > 50 * 1024 * 1024)
            return Results.BadRequest(ApiResult.Fail("File size exceeds 50MB limit"));

        // 验证文件类型（只允许图片和视频）
        var allowedTypes = new[]
        {
            "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp",
            "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo"
        };

        if (!allowedTypes.Contains(file.ContentType?.ToLower()))
            return Results.BadRequest(ApiResult.Fail("Invalid file type. Only images and videos are allowed."));

        try
        {
            // 上传文件
            using var stream = file.OpenReadStream();
            var fileName = await storageService.UploadFileAsync(
                file.FileName,
                stream,
                file.ContentType,
                CancellationToken.None);

            // 获取访问URL
            var url = await storageService.GetFileUrlAsync(fileName);

            logger.LogInformation("User {UserId} uploaded file: {FileName}", userId, fileName);

            return Results.Ok(new FileUploadResponse(fileName, url, file.ContentType, file.Length));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload file for user {UserId}", userId);
            return Results.Problem("Failed to upload file");
        }
    }

    private static async Task<IResult> DeleteFile(
        string fileName,
        ClaimsPrincipal user,
        [FromServices] IStorageService storageService,
        [FromServices] ILogger<IStorageService> logger)
    {
        if (!user.TryGetUserId(out var userId))
            return Results.Unauthorized();

        try
        {
            // 检查文件是否存在
            var exists = await storageService.FileExistsAsync(fileName);
            if (!exists)
                return Results.NotFound(ApiResult.NotFound("File not found"));

            // 删除文件
            await storageService.DeleteFileAsync(fileName);

            logger.LogInformation("User {UserId} deleted file: {FileName}", userId, fileName);

            return Results.Ok(new MessageResponse("File deleted successfully"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file {FileName} for user {UserId}", fileName, userId);
            return Results.Problem("Failed to delete file");
        }
    }

    private static async Task<IResult> GetFileUrl(
        string fileName,
        ClaimsPrincipal user,
        [FromServices] IStorageService storageService)
    {
        if (!user.TryGetUserId(out _))
            return Results.Unauthorized();

        try
        {
            // 检查文件是否存在
            var exists = await storageService.FileExistsAsync(fileName);
            if (!exists)
                return Results.NotFound(ApiResult.NotFound("File not found"));

            // 获取访问URL
            var url = await storageService.GetFileUrlAsync(fileName);

            return Results.Ok(new FileUrlResponse(url));
        }
        catch (Exception ex)
        {
            return Results.Problem("Failed to get file URL");
        }
    }
}

