using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System.Diagnostics;

namespace CatCat.Infrastructure.Payment;

/// <summary>
/// Stripe 支付服务接口
/// </summary>
public interface IPaymentService
{
    // Payment Intent
    Task<PaymentIntentResult> CreatePaymentIntentAsync(long orderId, decimal amount, string currency = "cny", CancellationToken cancellationToken = default);
    Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default);

    // Refunds
    Task<RefundResult> RefundPaymentAsync(string paymentIntentId, decimal? amount = null, string? reason = null, CancellationToken cancellationToken = default);
    Task<Refund?> GetRefundAsync(string refundId, CancellationToken cancellationToken = default);

    // Customers
    Task<Customer?> CreateCustomerAsync(long userId, string email, string? name = null, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<Customer?> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default);
    Task<Customer?> UpdateCustomerAsync(string customerId, string? email = null, string? name = null, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default);

    // Payment Methods
    Task<PaymentMethod?> AttachPaymentMethodAsync(string paymentMethodId, string customerId, CancellationToken cancellationToken = default);
    Task<PaymentMethod?> DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);
    Task<List<PaymentMethod>> ListCustomerPaymentMethodsAsync(string customerId, CancellationToken cancellationToken = default);

    // Charges
    Task<Charge?> GetChargeAsync(string chargeId, CancellationToken cancellationToken = default);

    // Balance & Payouts
    Task<Balance?> GetBalanceAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Stripe 支付服务实现（使用官方 Stripe.net SDK）
/// </summary>
public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;
    private readonly ActivitySource? _activitySource;

    // Stripe Services
    private readonly PaymentIntentService _paymentIntentService;
    private readonly RefundService _refundService;
    private readonly CustomerService _customerService;
    private readonly PaymentMethodService _paymentMethodService;
    private readonly ChargeService _chargeService;
    private readonly BalanceService _balanceService;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger,
        ActivitySource? activitySource = null)
    {
        _logger = logger;
        _activitySource = activitySource;

        // 配置 Stripe API Key
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey is not configured");

        // 初始化 Stripe 服务（缓存实例避免重复创建）
        _paymentIntentService = new PaymentIntentService();
        _refundService = new RefundService();
        _customerService = new CustomerService();
        _paymentMethodService = new PaymentMethodService();
        _chargeService = new ChargeService();
        _balanceService = new BalanceService();
    }

    #region Payment Intent

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        long orderId,
        decimal amount,
        string currency = "cny",
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.CreatePaymentIntent");
        activity?.SetTag("order.id", orderId);
        activity?.SetTag("payment.amount", amount);
        activity?.SetTag("payment.currency", currency);

        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Stripe 使用最小单位（分）
                Currency = currency,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId.ToString() },
                    { "source", "catcat-api" }
                },
                // 支付成功后自动捕获
                CaptureMethod = "automatic",
                // 确认方式（需要客户端确认）
                ConfirmationMethod = "automatic"
            };

            var paymentIntent = await _paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Stripe PaymentIntent created: PaymentIntentId={PaymentIntentId}, OrderId={OrderId}, Amount={Amount} {Currency}",
                paymentIntent.Id, orderId, amount, currency);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return new PaymentIntentResult
            {
                Success = true,
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status,
                Amount = paymentIntent.Amount / 100m,
                Currency = paymentIntent.Currency
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create PaymentIntent: OrderId={OrderId}, Error={Error}", 
                orderId, ex.Message);
            
            // AOT-compatible: Use pattern matching instead of reflection
            var exceptionType = ex switch
            {
                StripeException => nameof(StripeException),
                _ => "Exception"
            };
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.type", exceptionType },
                { "exception.message", ex.Message },
                { "exception.stacktrace", ex.StackTrace ?? string.Empty }
            }));

            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = ex.StripeError?.Code
            };
        }
    }

    public async Task<PaymentIntent?> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.GetPaymentIntent");
        activity?.SetTag("payment_intent.id", paymentIntentId);

        try
        {
            var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return paymentIntent;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get PaymentIntent: PaymentIntentId={PaymentIntentId}", paymentIntentId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    public async Task<bool> ConfirmPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.ConfirmPayment");
        activity?.SetTag("payment_intent.id", paymentIntentId);

        try
        {
            var paymentIntent = await _paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            var isSucceeded = paymentIntent.Status == "succeeded";

            _logger.LogInformation(
                "Payment confirmation checked: PaymentIntentId={PaymentIntentId}, Status={Status}, Success={Success}",
                paymentIntentId, paymentIntent.Status, isSucceeded);

            activity?.SetTag("payment.status", paymentIntent.Status);
            activity?.SetTag("payment.succeeded", isSucceeded);
            activity?.SetStatus(ActivityStatusCode.Ok);

            return isSucceeded;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to confirm payment: PaymentIntentId={PaymentIntentId}", paymentIntentId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    public async Task<bool> CancelPaymentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.CancelPayment");
        activity?.SetTag("payment_intent.id", paymentIntentId);

        try
        {
            var paymentIntent = await _paymentIntentService.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);

            _logger.LogInformation("Payment cancelled: PaymentIntentId={PaymentIntentId}", paymentIntentId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return paymentIntent.Status == "canceled";
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to cancel payment: PaymentIntentId={PaymentIntentId}", paymentIntentId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    #endregion

    #region Refunds

    public async Task<RefundResult> RefundPaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.RefundPayment");
        activity?.SetTag("payment_intent.id", paymentIntentId);
        activity?.SetTag("refund.amount", amount);
        activity?.SetTag("refund.reason", reason);

        try
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Reason = reason switch
                {
                    "duplicate" => "duplicate",
                    "fraudulent" => "fraudulent",
                    "requested_by_customer" => "requested_by_customer",
                    _ => null
                }
            };

            if (amount.HasValue)
            {
                options.Amount = (long)(amount.Value * 100);
            }

            var refund = await _refundService.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Refund created: RefundId={RefundId}, PaymentIntentId={PaymentIntentId}, Amount={Amount}, Status={Status}",
                refund.Id, paymentIntentId, refund.Amount / 100m, refund.Status);

            activity?.SetStatus(ActivityStatusCode.Ok);

            return new RefundResult
            {
                Success = true,
                RefundId = refund.Id,
                Status = refund.Status,
                Amount = refund.Amount / 100m,
                Currency = refund.Currency
            };
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to refund payment: PaymentIntentId={PaymentIntentId}", paymentIntentId);
            
            // AOT-compatible: Use pattern matching instead of reflection
            var exceptionType = ex switch
            {
                StripeException => nameof(StripeException),
                _ => "Exception"
            };
            
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.type", exceptionType },
                { "exception.message", ex.Message },
                { "exception.stacktrace", ex.StackTrace ?? string.Empty }
            }));

            return new RefundResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = ex.StripeError?.Code
            };
        }
    }

    public async Task<Refund?> GetRefundAsync(string refundId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.GetRefund");
        activity?.SetTag("refund.id", refundId);

        try
        {
            var refund = await _refundService.GetAsync(refundId, cancellationToken: cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return refund;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get refund: RefundId={RefundId}", refundId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    #endregion

    #region Customers

    public async Task<Customer?> CreateCustomerAsync(
        long userId,
        string email,
        string? name = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.CreateCustomer");
        activity?.SetTag("user.id", userId);
        activity?.SetTag("customer.email", email);

        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = metadata ?? new Dictionary<string, string>(),
            };

            // 添加用户ID到元数据
            options.Metadata["user_id"] = userId.ToString();

            var customer = await _customerService.CreateAsync(options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Stripe Customer created: CustomerId={CustomerId}, UserId={UserId}, Email={Email}",
                customer.Id, userId, email);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return customer;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to create customer: UserId={UserId}, Email={Email}", userId, email);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    public async Task<Customer?> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.GetCustomer");
        activity?.SetTag("customer.id", customerId);

        try
        {
            var customer = await _customerService.GetAsync(customerId, cancellationToken: cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return customer;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get customer: CustomerId={CustomerId}", customerId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    public async Task<Customer?> UpdateCustomerAsync(
        string customerId,
        string? email = null,
        string? name = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.UpdateCustomer");
        activity?.SetTag("customer.id", customerId);

        try
        {
            var options = new CustomerUpdateOptions();

            if (email != null) options.Email = email;
            if (name != null) options.Name = name;
            if (metadata != null) options.Metadata = metadata;

            var customer = await _customerService.UpdateAsync(customerId, options, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe Customer updated: CustomerId={CustomerId}", customerId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return customer;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to update customer: CustomerId={CustomerId}", customerId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    public async Task<bool> DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.DeleteCustomer");
        activity?.SetTag("customer.id", customerId);

        try
        {
            await _customerService.DeleteAsync(customerId, cancellationToken: cancellationToken);

            _logger.LogInformation("Stripe Customer deleted: CustomerId={CustomerId}", customerId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return true;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to delete customer: CustomerId={CustomerId}", customerId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return false;
        }
    }

    #endregion

    #region Payment Methods

    public async Task<PaymentMethod?> AttachPaymentMethodAsync(
        string paymentMethodId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.AttachPaymentMethod");
        activity?.SetTag("payment_method.id", paymentMethodId);
        activity?.SetTag("customer.id", customerId);

        try
        {
            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId
            };

            var paymentMethod = await _paymentMethodService.AttachAsync(paymentMethodId, options, cancellationToken: cancellationToken);

            _logger.LogInformation(
                "PaymentMethod attached: PaymentMethodId={PaymentMethodId}, CustomerId={CustomerId}",
                paymentMethodId, customerId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return paymentMethod;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to attach payment method: PaymentMethodId={PaymentMethodId}", paymentMethodId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    public async Task<PaymentMethod?> DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.DetachPaymentMethod");
        activity?.SetTag("payment_method.id", paymentMethodId);

        try
        {
            var paymentMethod = await _paymentMethodService.DetachAsync(paymentMethodId, cancellationToken: cancellationToken);

            _logger.LogInformation("PaymentMethod detached: PaymentMethodId={PaymentMethodId}", paymentMethodId);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return paymentMethod;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to detach payment method: PaymentMethodId={PaymentMethodId}", paymentMethodId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    public async Task<List<PaymentMethod>> ListCustomerPaymentMethodsAsync(string customerId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.ListPaymentMethods");
        activity?.SetTag("customer.id", customerId);

        try
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card" // 支持卡片支付
            };

            var paymentMethods = await _paymentMethodService.ListAsync(options, cancellationToken: cancellationToken);

            activity?.SetStatus(ActivityStatusCode.Ok);
            return paymentMethods.Data;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to list payment methods: CustomerId={CustomerId}", customerId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return new List<PaymentMethod>();
        }
    }

    #endregion

    #region Charges

    public async Task<Charge?> GetChargeAsync(string chargeId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.GetCharge");
        activity?.SetTag("charge.id", chargeId);

        try
        {
            var charge = await _chargeService.GetAsync(chargeId, cancellationToken: cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return charge;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get charge: ChargeId={ChargeId}", chargeId);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    #endregion

    #region Balance & Payouts

    public async Task<Balance?> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource?.StartActivity("Stripe.GetBalance");

        try
        {
            var balance = await _balanceService.GetAsync(cancellationToken: cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return balance;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Failed to get balance");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            return null;
        }
    }

    #endregion
}

/// <summary>
/// Payment Intent 创建结果
/// </summary>
public record PaymentIntentResult
{
    public bool Success { get; init; }
    public string? PaymentIntentId { get; init; }
    public string? ClientSecret { get; init; }
    public string? Status { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
}

/// <summary>
/// 退款结果
/// </summary>
public record RefundResult
{
    public bool Success { get; init; }
    public string? RefundId { get; init; }
    public string? Status { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
}
