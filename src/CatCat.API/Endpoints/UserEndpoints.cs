using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class UserEndpoints
{
    [RequiresUnreferencedCode("Uses JSON serialization which may require unreferenced code")]
    [RequiresDynamicCode("Uses JSON serialization which may require dynamic code")]
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", async (ClaimsPrincipal user, IUserRepository userRepository) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var userInfo = await userRepository.GetByIdAsync(userId);
            return userInfo == null
                ? Results.NotFound(ApiResult.NotFound("用户不存在"))
                : Results.Ok(userInfo);
        })
        .WithName("GetCurrentUser")
        .WithSummary("获取当前用户信息");

        group.MapPut("/me", async (ClaimsPrincipal user, [FromBody] UpdateUserRequest request, IUserRepository userRepository) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var userInfo = await userRepository.GetByIdAsync(userId);
            if (userInfo == null)
                return Results.NotFound(ApiResult.NotFound("用户不存在"));

            if (!string.IsNullOrEmpty(request.NickName)) userInfo.NickName = request.NickName;
            if (!string.IsNullOrEmpty(request.Email)) userInfo.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Avatar)) userInfo.Avatar = request.Avatar;
            userInfo.UpdatedAt = DateTime.UtcNow;

            await userRepository.UpdateAsync(userInfo);
            return Results.Ok(new MessageResponse("更新成功"));
        })
        .WithName("UpdateCurrentUser")
        .WithSummary("更新当前用户信息");

        group.MapGet("/", async (ClaimsPrincipal user, [FromQuery] int page, [FromQuery] int pageSize, IUserRepository userRepository) =>
        {
            if (!user.IsInRole("Admin"))
                return Results.Forbid();

            var items = await userRepository.GetPagedAsync((page - 1) * pageSize, pageSize);
            var total = await userRepository.GetCountAsync();
            return Results.Ok(new UserListResponse(items, total, page, pageSize));
        })
        .WithName("GetUserList")
        .WithSummary("获取用户列表（管理员）");
    }
}
