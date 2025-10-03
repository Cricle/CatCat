using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatCat.API.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Statistics
        group.MapGet("/statistics", GetStatistics);

        // User Management
        group.MapGet("/users", GetUsers);
        group.MapPut("/users/{id}/status", UpdateUserStatus);
        group.MapPut("/users/{id}/role", UpdateUserRole);

        // Pet Management
        group.MapGet("/pets", GetAllPets);
        group.MapDelete("/pets/{id}", DeletePet);

        // Package Management
        group.MapGet("/packages", GetPackages);
        group.MapPost("/packages", CreatePackage);
        group.MapPut("/packages/{id}", UpdatePackage);
        group.MapDelete("/packages/{id}", DeletePackage);
        group.MapPatch("/packages/{id}/toggle", TogglePackageStatus);
    }

    // Statistics
    private static async Task<IResult> GetStatistics(
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var result = await adminService.GetStatisticsAsync(cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    // User Management
    private static async Task<IResult> GetUsers(
        IAdminService adminService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] UserRole? role = null,
        [FromQuery] UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetUsersAsync(page, pageSize, role, status, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> UpdateUserStatus(
        long id,
        [FromBody] UpdateUserStatusRequest request,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateUserStatusAsync(id, request.Status, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("User status updated"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> UpdateUserRole(
        long id,
        [FromBody] UpdateUserRoleRequest request,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var result = await adminService.UpdateUserRoleAsync(id, request.Role, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("User role updated"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    // Pet Management
    private static async Task<IResult> GetAllPets(
        IAdminService adminService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] long? userId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await adminService.GetAllPetsAsync(page, pageSize, userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> DeletePet(
        long id,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var result = await adminService.DeletePetAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Pet deleted"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    // Package Management
    private static async Task<IResult> GetPackages(
        IServicePackageService packageService,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await packageService.GetPagedAsync(page, pageSize, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> CreatePackage(
        [FromBody] CreatePackageRequest request,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var command = new CreatePackageCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Duration,
            request.IconUrl,
            request.ServiceItems,
            request.SortOrder);

        var result = await adminService.CreatePackageAsync(command, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!, "Package created"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> UpdatePackage(
        long id,
        [FromBody] UpdatePackageRequest request,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePackageCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.Duration,
            request.IconUrl,
            request.ServiceItems,
            request.IsActive,
            request.SortOrder);

        var result = await adminService.UpdatePackageAsync(command, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Package updated"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> DeletePackage(
        long id,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var result = await adminService.DeletePackageAsync(id, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Package deleted"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> TogglePackageStatus(
        long id,
        [FromBody] TogglePackageStatusRequest request,
        IAdminService adminService,
        CancellationToken cancellationToken)
    {
        var result = await adminService.TogglePackageStatusAsync(id, request.IsActive, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Package status toggled"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }
}

// Request models
public record UpdateUserStatusRequest(UserStatus Status);
public record UpdateUserRoleRequest(UserRole Role);
public record CreatePackageRequest(
    string Name,
    string Description,
    decimal Price,
    int Duration,
    string? IconUrl,
    string ServiceItems,
    int SortOrder);
public record UpdatePackageRequest(
    string Name,
    string Description,
    decimal Price,
    int Duration,
    string? IconUrl,
    string ServiceItems,
    bool IsActive,
    int SortOrder);
public record TogglePackageStatusRequest(bool IsActive);

