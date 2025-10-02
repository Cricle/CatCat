using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization()
            .WithOpenApi();

        // 获取当前用户信息
        group.MapGet("/me", async (
            ClaimsPrincipal user,
            IUserRepository userRepository) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var userInfo = await userRepository.GetByIdAsync(userId);
            if (userInfo == null)
            {
                return Results.NotFound(new { message = "用户不存在" });
            }

            return Results.Ok(new
            {
                userInfo.Id,
                userInfo.Phone,
                userInfo.Email,
                userInfo.NickName,
                userInfo.Avatar,
                userInfo.Role,
                userInfo.Status,
                userInfo.CreatedAt
            });
        })
        .WithName("GetCurrentUser")
        .WithSummary("获取当前用户信息");

        // 更新用户信息
        group.MapPut("/me", async (
            ClaimsPrincipal user,
            [FromBody] UpdateUserRequest request,
            IUserRepository userRepository) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var userInfo = await userRepository.GetByIdAsync(userId);
            if (userInfo == null)
            {
                return Results.NotFound(new { message = "用户不存在" });
            }

            // 更新字段
            if (!string.IsNullOrEmpty(request.NickName))
                userInfo.NickName = request.NickName;
            if (!string.IsNullOrEmpty(request.Email))
                userInfo.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Avatar))
                userInfo.Avatar = request.Avatar;

            userInfo.UpdatedAt = DateTime.UtcNow;

            await userRepository.UpdateAsync(userInfo);

            return Results.Ok(new { message = "更新成功" });
        })
        .WithName("UpdateCurrentUser")
        .WithSummary("更新当前用户信息");

        // 获取用户列表（管理员）
        group.MapGet("/", async (
            ClaimsPrincipal user,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IUserRepository userRepository) =>
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            if (roleClaim?.Value != "Admin")
            {
                return Results.Forbid();
            }

            var offset = (page - 1) * pageSize;
            var items = await userRepository.GetPagedAsync(offset, pageSize);
            var total = await userRepository.GetCountAsync();

            return Results.Ok(new
            {
                items,
                total,
                page,
                pageSize
            });
        })
        .WithName("GetUserList")
        .WithSummary("获取用户列表（管理员）");
    }
}

public record UpdateUserRequest(string? NickName, string? Email, string? Avatar);

