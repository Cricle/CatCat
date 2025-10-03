using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CatCat.API.Endpoints;

public static class ServiceProgressEndpoints
{
    public static void MapServiceProgressEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/service-progress")
            .WithTags("Service Progress")
            .RequireAuthorization();

        group.MapGet("/order/{orderId}", GetOrderProgress);
        group.MapGet("/order/{orderId}/latest", GetLatestProgress);
        group.MapPost("", CreateProgress);
    }

    private static async Task<IResult> GetOrderProgress(
        long orderId,
        IServiceProgressService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetOrderProgressAsync(orderId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> GetLatestProgress(
        long orderId,
        IServiceProgressService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetLatestProgressAsync(orderId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!))
            : Results.NotFound(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> CreateProgress(
        [FromBody] CreateServiceProgressRequest request,
        IServiceProgressService service,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = context.User.GetUserId();

        var command = new CreateProgressCommand(
            request.OrderId,
            userId,
            request.Status,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.Address,
            request.ImageUrls);

        var result = await service.CreateProgressAsync(command, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value!, "Progress created successfully"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }
}

public record CreateServiceProgressRequest(
    long OrderId,
    ServiceProgressStatus Status,
    string? Description,
    double? Latitude,
    double? Longitude,
    string? Address,
    string? ImageUrls);

