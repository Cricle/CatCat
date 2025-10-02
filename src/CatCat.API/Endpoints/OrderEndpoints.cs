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
    [RequiresUnreferencedCode("Uses JSON serialization which may require unreferenced code")]
    [RequiresDynamicCode("Uses JSON serialization which may require dynamic code")]
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        group.MapPost("", async (
            [FromBody] CreateOrderRequest request,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
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
                ? Results.Ok(ApiResult.Ok(result.Value, "订单创建成功"))
                : Results.BadRequest(ApiResult.Fail<long>(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("order-create")
        .WithName("CreateOrder");

        group.MapGet("{id:long}", async (
            long id,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            var result = await orderService.GetOrderDetailAsync(id, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok(result.Value))
                : Results.NotFound(ApiResult.NotFound(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("query")
        .WithName("GetOrderDetail");

        group.MapGet("/my", async (ClaimsPrincipal user, IOrderService orderService, CancellationToken cancellationToken,
            [FromQuery] int? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            if (!user.TryGetUserId(out var customerId))
                return Results.Unauthorized();

            var result = await orderService.GetCustomerOrdersAsync(customerId, status, page, pageSize, cancellationToken);
            if (!result.IsSuccess)
                return Results.BadRequest(ApiResult.Fail<object>(result.Error!));

            var (items, total) = result.Value;
            var pagedResult = PagedResult<ServiceOrder>.Create(items, total, page, pageSize);
            return Results.Ok(ApiResult.Ok(pagedResult));
        })
        .RequireAuthorization()
        .RequireRateLimiting("query")
        .WithName("GetMyOrders");

        group.MapPost("{id:long}/cancel", async (long id, ClaimsPrincipal user, IOrderService orderService, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var result = await orderService.CancelOrderAsync(id, userId, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("订单已取消"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")
        .WithName("CancelOrder");

        group.MapPost("{id:long}/accept", async (long id, ClaimsPrincipal user, IOrderService orderService, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetUserId(out var providerId))
                return Results.Unauthorized();

            var result = await orderService.AcceptOrderAsync(id, providerId, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("接单成功"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")
        .WithName("AcceptOrder");

        group.MapPost("{id:long}/start", async (long id, ClaimsPrincipal user, IOrderService orderService, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetUserId(out var providerId))
                return Results.Unauthorized();

            var result = await orderService.StartServiceAsync(id, providerId, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("服务已开始"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")
        .WithName("StartService");

        group.MapPost("{id:long}/complete", async (long id, ClaimsPrincipal user, IOrderService orderService, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetUserId(out var providerId))
                return Results.Unauthorized();

            var result = await orderService.CompleteServiceAsync(id, providerId, cancellationToken);
            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("服务已完成"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")
        .WithName("CompleteService");


        group.MapPost("{id:long}/pay", async (
            long id,
            [FromBody] PayOrderRequest request,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            var result = await orderService.PayOrderAsync(id, request.PaymentIntentId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("支付成功"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("payment")
        .WithName("PayOrder");
    }
}
