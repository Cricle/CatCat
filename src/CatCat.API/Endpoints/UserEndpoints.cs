using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .WithSummary("Get current user info");

        group.MapPut("/me", UpdateCurrentUser)
            .WithName("UpdateCurrentUser")
            .WithSummary("Update current user info");

        group.MapGet("/", GetUserList)
            .WithName("GetUserList")
            .WithSummary("Get user list (Admin only)");
    }

    private static async Task<IResult> GetCurrentUser(ClaimsPrincipal user, IUserRepository userRepository)
    {
        if (!user.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var userInfo = await userRepository.GetByIdAsync(userId);
        return userInfo == null
            ? Results.NotFound(ApiResult.NotFound("User not found"))
            : Results.Ok(userInfo);
    }

    private static async Task<IResult> UpdateCurrentUser(
        ClaimsPrincipal user,
        [FromBody] UpdateUserRequest request,
        IUserRepository userRepository)
    {
        if (!user.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var userInfo = await userRepository.GetByIdAsync(userId);
        if (userInfo == null)
            return Results.NotFound(ApiResult.NotFound("User not found"));

        if (!string.IsNullOrEmpty(request.NickName)) userInfo.NickName = request.NickName;
        if (!string.IsNullOrEmpty(request.Email)) userInfo.Email = request.Email;
        if (!string.IsNullOrEmpty(request.Avatar)) userInfo.Avatar = request.Avatar;
        userInfo.UpdatedAt = DateTime.UtcNow;

        await userRepository.UpdateAsync(userInfo);
        return Results.Ok(new MessageResponse("Update successful"));
    }

    private static async Task<IResult> GetUserList(
        ClaimsPrincipal user,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        IUserRepository userRepository)
    {
        if (!user.IsInRole("Admin"))
            return Results.Forbid();

        var items = await userRepository.GetPagedAsync((page - 1) * pageSize, pageSize);
        var total = await userRepository.GetCountAsync();
        return Results.Ok(new UserListResponse(items, total, page, pageSize));
    }
}
