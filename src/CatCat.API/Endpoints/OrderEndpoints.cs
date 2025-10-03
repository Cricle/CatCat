using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Services;
using CatCat.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CatCat.API.Endpoints;
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("", CreateOrder)
            .RequireAuthorization()
            .RequireRateLimiting("order-create")
            .WithName("CreateOrder");

        group.MapGet("{id:long}", GetOrderDetail)
            .RequireAuthorization()
            .RequireRateLimiting("query")
            .WithName("GetOrderDetail");

        group.MapGet("/my", GetMyOrders)
            .RequireAuthorization()
            .RequireRateLimiting("query")
            .WithName("GetMyOrders");

        group.MapPost("{id:long}/cancel", CancelOrder)
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithName("CancelOrder");

        group.MapPost("{id:long}/accept", AcceptOrder)
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithName("AcceptOrder");

        group.MapPost("{id:long}/start", StartService)
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithName("StartService");

        group.MapPost("{id:long}/complete", CompleteService)
            .RequireAuthorization()
            .RequireRateLimiting("api")
            .WithName("CompleteService");

        group.MapPost("{id:long}/pay", PayOrder)
            .RequireAuthorization()
            .RequireRateLimiting("payment")
            .WithName("PayOrder");
    }

    private static async Task<IResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromServices] IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.CustomerId,
            request.ServicePackageId,
            request.PetId,
            request.ServiceDate,
            request.ServiceAddress,
            request.Remark);

        var result = await orderService.CreateOrderAsync(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value, "Order submitted, processing..."))
            : Results.BadRequest(ApiResult.Fail<long>(result.Error!));
    }

    private static async Task<IResult> GetOrderDetail(
        long id,
        [FromServices] IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetOrderDetailAsync(id, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok(result.Value))
            : Results.NotFound(ApiResult.NotFound(result.Error!));
    }

    private static async Task<IResult> GetMyOrders(
        ClaimsPrincipal user,
        IOrderService orderService,
        CancellationToken cancellationToken,
        [FromQuery] int? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!user.TryGetUserId(out var customerId))
            return Results.Unauthorized();

        var result = await orderService.GetCustomerOrdersAsync(customerId, status, page, pageSize, cancellationToken);
        if (!result.IsSuccess || result.Value == null)
            return Results.BadRequest(ApiResult.Fail<object>(result.Error!));

        var pagedData = result.Value;
        var pagedResult = PagedResult<ServiceOrder>.Create(pagedData.Items, pagedData.Total, page, pageSize);
        return Results.Ok(ApiResult.Ok(pagedResult));
    }

    private static async Task<IResult> CancelOrder(
        long id,
        ClaimsPrincipal user,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        if (!user.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var result = await orderService.CancelOrderAsync(id, userId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Order cancelled"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> AcceptOrder(
        long id,
        ClaimsPrincipal user,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        if (!user.TryGetUserId(out var providerId))
            return Results.Unauthorized();

        var result = await orderService.AcceptOrderAsync(id, providerId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Order accepted"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> StartService(
        long id,
        ClaimsPrincipal user,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        if (!user.TryGetUserId(out var providerId))
            return Results.Unauthorized();

        var result = await orderService.StartServiceAsync(id, providerId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Service started"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> CompleteService(
        long id,
        ClaimsPrincipal user,
        IOrderService orderService,
        CancellationToken cancellationToken)
    {
        if (!user.TryGetUserId(out var providerId))
            return Results.Unauthorized();

        var result = await orderService.CompleteServiceAsync(id, providerId, cancellationToken);
        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Service completed"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }

    private static async Task<IResult> PayOrder(
        long id,
        [FromBody] PayOrderRequest request,
        [FromServices] IOrderService orderService,
        CancellationToken cancellationToken)
    {
        var result = await orderService.PayOrderAsync(id, request.PaymentIntentId, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResult.Ok("Payment successful"))
            : Results.BadRequest(ApiResult.Fail(result.Error!));
    }
}
