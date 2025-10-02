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
    private readonly PaymentIntentService _paymentIntentService;
    private readonly RefundService _refundService;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _logger = logger;
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

        // Cache Stripe service instances to avoid repeated creation
        _paymentIntentService = new PaymentIntentService();
        _refundService = new RefundService();
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(long orderId, decimal amount, string currency = "usd")
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe uses smallest unit (cents)
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

            var paymentIntent = await _paymentIntentService.CreateAsync(options);

            _logger.LogInformation("Stripe PaymentIntent created successfully: {PaymentIntentId}, OrderId: {OrderId}",
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
            _logger.LogError(ex, "Failed to create PaymentIntent: OrderId={OrderId}", orderId);
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
            var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId);

            _logger.LogInformation("Payment confirmed: {PaymentIntentId}, Status: {Status}",
                paymentIntentId, paymentIntent.Status);

            return paymentIntent.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to confirm payment: PaymentIntentId={PaymentIntentId}", paymentIntentId);
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

            var refund = await _refundService.CreateAsync(options);

            _logger.LogInformation("Refund successful: {RefundId}, PaymentIntentId: {PaymentIntentId}",
                refund.Id, paymentIntentId);

            return refund.Status == "succeeded";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to refund payment: PaymentIntentId={PaymentIntentId}", paymentIntentId);
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

