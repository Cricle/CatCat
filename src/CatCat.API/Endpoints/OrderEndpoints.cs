using CatCat.API.Models;
using CatCat.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CatCat.API.Endpoints;

/// <summary>
/// 订单 Endpoints - 使用限流策略防止击穿
/// </summary>
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders");

        // 创建订单 - 令牌桶限流（允许突发但总体受限）
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
        .RequireRateLimiting("order-create")  // 令牌桶限流
        .WithName("CreateOrder")
        .WithOpenApi();

        // 获取订单详情 - 查询限流（较宽松）
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
        .RequireRateLimiting("query")  // 查询限流
        .WithName("GetOrderDetail")
        .WithOpenApi();

        // 获取用户订单列表 - 查询限流
        group.MapGet("/my", async (
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken,
            [FromQuery] int? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            // TODO: 从 JWT 获取用户 ID
            long customerId = 1;

            var result = await orderService.GetCustomerOrdersAsync(
                customerId, status, page, pageSize, cancellationToken);

            if (result.IsSuccess)
            {
                var (items, total) = result.Value;
                var pagedResult = PagedResult<Domain.Entities.ServiceOrder>.Create(
                    items, total, page, pageSize);
                return Results.Ok(ApiResult.Ok(pagedResult));
            }

            return Results.BadRequest(ApiResult.Fail<object>(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("query")  // 查询限流
        .WithName("GetMyOrders")
        .WithOpenApi();

        // 取消订单 - API 标准限流
        group.MapPost("{id:long}/cancel", async (
            long id,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            // TODO: 从 JWT 获取用户 ID
            long userId = 1;

            var result = await orderService.CancelOrderAsync(id, userId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("订单已取消"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")  // 标准限流
        .WithName("CancelOrder")
        .WithOpenApi();

        // 服务商接单 - API 标准限流
        group.MapPost("{id:long}/accept", async (
            long id,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            // TODO: 从 JWT 获取服务商 ID
            long providerId = 1;

            var result = await orderService.AcceptOrderAsync(id, providerId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("接单成功"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")  // 标准限流
        .WithName("AcceptOrder")
        .WithOpenApi();

        // 开始服务 - API 标准限流
        group.MapPost("{id:long}/start", async (
            long id,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            // TODO: 从 JWT 获取服务商 ID
            long providerId = 1;

            var result = await orderService.StartServiceAsync(id, providerId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("服务已开始"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")  // 标准限流
        .WithName("StartService")
        .WithOpenApi();

        // 完成服务 - API 标准限流
        group.MapPost("{id:long}/complete", async (
            long id,
            [FromServices] IOrderService orderService,
            CancellationToken cancellationToken) =>
        {
            // TODO: 从 JWT 获取服务商 ID
            long providerId = 1;

            var result = await orderService.CompleteServiceAsync(id, providerId, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(ApiResult.Ok("服务已完成"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .RequireRateLimiting("api")  // 标准限流
        .WithName("CompleteService")
        .WithOpenApi();

        // 支付订单 - 并发限流（严格控制，同一用户同时只能有1个支付请求）
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
        .RequireRateLimiting("payment")  // 并发限流
        .WithName("PayOrder")
        .WithOpenApi();
    }
}

public record CreateOrderRequest(
    long CustomerId,
    long ServicePackageId,
    long PetId,
    DateTime ServiceDate,
    string ServiceAddress,
    string? Remark);

public record PayOrderRequest(string PaymentIntentId);
