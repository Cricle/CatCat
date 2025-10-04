using Microsoft.AspNetCore.Mvc;
using Stripe;
using CatCat.Infrastructure.Payment;
using CatCat.Infrastructure.Services;
using System.Diagnostics;

namespace CatCat.API.Endpoints;

/// <summary>
/// Stripe Webhook 端点 - 处理 Stripe 事件
/// </summary>
public static class StripeWebhookEndpoints
{
    public static void MapStripeWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/webhooks/stripe")
            .WithTags("Stripe Webhooks")
            .AllowAnonymous(); // Webhook 不需要认证，通过签名验证

        group.MapPost("", HandleStripeWebhook)
            .WithName("HandleStripeWebhook");
    }

    private static async Task<IResult> HandleStripeWebhook(
        HttpRequest request,
        [FromServices] IConfiguration configuration,
        [FromServices] IOrderService orderService,
        [FromServices] ILogger<Program> logger,
        [FromServices] ActivitySource? activitySource = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = activitySource?.StartActivity("Stripe.Webhook");

        try
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync(cancellationToken);
            var webhookSecret = configuration["Stripe:WebhookSecret"];

            Event? stripeEvent;

            try
            {
                // 验证 Webhook 签名
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    request.Headers["Stripe-Signature"],
                    webhookSecret,
                    throwOnApiVersionMismatch: false
                );

                activity?.SetTag("stripe.event.id", stripeEvent.Id);
                activity?.SetTag("stripe.event.type", stripeEvent.Type);

                logger.LogInformation("Stripe webhook received: EventId={EventId}, Type={Type}",
                    stripeEvent.Id, stripeEvent.Type);
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Invalid Stripe webhook signature");
                activity?.SetStatus(ActivityStatusCode.Error, "Invalid signature");
                return Results.BadRequest(new { error = "Invalid signature" });
            }

            // 处理不同类型的事件
            switch (stripeEvent.Type)
            {
                // Payment Intent 成功
                case "payment_intent.succeeded":
                    await HandlePaymentIntentSucceeded(stripeEvent, orderService, logger, cancellationToken);
                    break;

                // Payment Intent 失败
                case "payment_intent.payment_failed":
                    await HandlePaymentIntentFailed(stripeEvent, logger);
                    break;

                // Payment Intent 取消
                case "payment_intent.canceled":
                    await HandlePaymentIntentCanceled(stripeEvent, logger);
                    break;

                // 退款成功
                case "charge.refunded":
                    await HandleChargeRefunded(stripeEvent, orderService, logger, cancellationToken);
                    break;

                // 退款更新
                case "charge.refund.updated":
                    await HandleChargeRefundUpdated(stripeEvent, logger);
                    break;

                // 支付方法附加
                case "payment_method.attached":
                    await HandlePaymentMethodAttached(stripeEvent, logger);
                    break;

                // 客户创建
                case "customer.created":
                    await HandleCustomerCreated(stripeEvent, logger);
                    break;

                // 客户删除
                case "customer.deleted":
                    await HandleCustomerDeleted(stripeEvent, logger);
                    break;

                default:
                    logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
                    break;
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            return Results.Ok(new { received = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook");

            // AOT-compatible: Use pattern matching instead of reflection
            var exceptionType = ex switch
            {
                StripeException => nameof(StripeException),
                InvalidOperationException => nameof(InvalidOperationException),
                ArgumentException => nameof(ArgumentException),
                _ => "Exception"
            };

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.type", exceptionType },
                { "exception.message", ex.Message }
            }));

            // 返回 200 避免 Stripe 重试（已记录错误）
            return Results.Ok(new { received = true, error = ex.Message });
        }
    }

    #region Event Handlers

    private static async Task HandlePaymentIntentSucceeded(
        Event stripeEvent,
        IOrderService orderService,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        if (paymentIntent == null) return;

        var orderId = paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) && long.TryParse(orderIdStr, out var id)
            ? id
            : (long?)null;

        logger.LogInformation(
            "Payment succeeded: PaymentIntentId={PaymentIntentId}, OrderId={OrderId}, Amount={Amount}",
            paymentIntent.Id, orderId, paymentIntent.Amount / 100m);

        if (orderId.HasValue)
        {
            try
            {
                // 更新订单支付状态
                var result = await orderService.PayOrderAsync(orderId.Value, paymentIntent.Id, cancellationToken);
                if (!result.IsSuccess)
                {
                    logger.LogWarning("Failed to update order payment status: OrderId={OrderId}, Error={Error}",
                        orderId.Value, result.Error);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating order payment: OrderId={OrderId}", orderId.Value);
            }
        }
    }

    private static Task HandlePaymentIntentFailed(Event stripeEvent, ILogger logger) =>
        LogPaymentIntent<PaymentIntent>(stripeEvent, logger, (pi, orderId) =>
            logger.LogWarning("Payment failed: PaymentIntentId={PaymentIntentId}, OrderId={OrderId}, Error={Error}",
                pi.Id, orderId, pi.LastPaymentError?.Message));

    private static Task HandlePaymentIntentCanceled(Event stripeEvent, ILogger logger) =>
        LogPaymentIntent<PaymentIntent>(stripeEvent, logger, (pi, orderId) =>
            logger.LogInformation("Payment canceled: PaymentIntentId={PaymentIntentId}, OrderId={OrderId}",
                pi.Id, orderId));

    private static Task HandleChargeRefunded(Event stripeEvent, IOrderService orderService, ILogger logger, CancellationToken cancellationToken) =>
        LogStripeEvent<Charge>(stripeEvent, logger, charge =>
            logger.LogInformation("Charge refunded: ChargeId={ChargeId}, Amount={Amount}, Refunded={Refunded}",
                charge.Id, charge.Amount / 100m, charge.Refunded));

    private static Task HandleChargeRefundUpdated(Event stripeEvent, ILogger logger) =>
        LogStripeEvent<Refund>(stripeEvent, logger, refund =>
            logger.LogInformation("Refund updated: RefundId={RefundId}, Status={Status}, Amount={Amount}",
                refund.Id, refund.Status, refund.Amount / 100m));

    private static Task HandlePaymentMethodAttached(Event stripeEvent, ILogger logger) =>
        LogStripeEvent<PaymentMethod>(stripeEvent, logger, pm =>
            logger.LogInformation("PaymentMethod attached: PaymentMethodId={PaymentMethodId}, CustomerId={CustomerId}, Type={Type}",
                pm.Id, pm.CustomerId, pm.Type));

    private static Task HandleCustomerCreated(Event stripeEvent, ILogger logger) =>
        LogStripeEvent<Customer>(stripeEvent, logger, customer =>
        {
            var userId = customer.Metadata.TryGetValue("user_id", out var userIdStr) ? userIdStr : "Unknown";
            logger.LogInformation("Stripe Customer created: CustomerId={CustomerId}, UserId={UserId}, Email={Email}",
                customer.Id, userId, customer.Email);
        });

    private static Task HandleCustomerDeleted(Event stripeEvent, ILogger logger) =>
        LogStripeEvent<Customer>(stripeEvent, logger, customer =>
            logger.LogInformation("Stripe Customer deleted: CustomerId={CustomerId}", customer.Id));

    // Helper methods to reduce duplication
    private static Task LogStripeEvent<T>(Event stripeEvent, ILogger logger, Action<T> logAction) where T : class
    {
        var obj = stripeEvent.Data.Object as T;
        if (obj != null) logAction(obj);
        return Task.CompletedTask;
    }

    private static Task LogPaymentIntent<T>(Event stripeEvent, ILogger logger, Action<T, string> logAction) where T : PaymentIntent
    {
        var pi = stripeEvent.Data.Object as T;
        if (pi != null)
        {
            var orderId = pi.Metadata.TryGetValue("order_id", out var orderIdStr) ? orderIdStr : "Unknown";
            logAction(pi, orderId);
        }
        return Task.CompletedTask;
    }

    #endregion
}

