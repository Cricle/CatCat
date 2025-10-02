using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace CatCat.Infrastructure.Payment;

public interface IPaymentService
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(long orderId, decimal amount, string currency = "usd");
    Task<bool> ConfirmPaymentAsync(string paymentIntentId);
    Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
}

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _logger = logger;
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(long orderId, decimal amount, string currency = "usd")
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe使用最小单位（美分）
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation("Stripe Payment Intent创建成功: {PaymentIntentId}, OrderId: {OrderId}",
                paymentIntent.Id, orderId);

            return new PaymentIntentResult
            {
                Success = true,
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "创建Payment Intent失败: OrderId={OrderId}", orderId);
            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            _logger.LogInformation("支付确认: {PaymentIntentId}, Status: {Status}",
                paymentIntentId, paymentIntent.Status);

            return paymentIntent.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "确认支付失败: PaymentIntentId={PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    public async Task<bool> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
    {
        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId
            };

            if (amount.HasValue)
            {
                options.Amount = (long)(amount.Value * 100);
            }

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            _logger.LogInformation("退款成功: {RefundId}, PaymentIntentId: {PaymentIntentId}",
                refund.Id, paymentIntentId);

            return refund.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "退款失败: PaymentIntentId={PaymentIntentId}", paymentIntentId);
            return false;
        }
    }
}

public record PaymentIntentResult
{
    public bool Success { get; init; }
    public string? PaymentIntentId { get; init; }
    public string? ClientSecret { get; init; }
    public string? ErrorMessage { get; init; }
}

