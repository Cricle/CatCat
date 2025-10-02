using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Messages;
using CatCat.Infrastructure.MessageQueue;
using CatCat.Infrastructure.Payment;
using CatCat.Infrastructure.Repositories;
using CatCat.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.API.BackgroundServices;

public class OrderProcessingService : BackgroundService
{
    private readonly IServiceOrderRepository _orderRepository;
    private readonly IOrderStatusHistoryRepository _historyRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IServicePackageRepository _packageRepository;
    private readonly IMessageQueueService _messageQueue;
    private readonly IPaymentService _paymentService;
    private readonly IFusionCache _cache;
    private readonly ILogger<OrderProcessingService> _logger;

    public OrderProcessingService(
        IServiceOrderRepository orderRepository,
        IOrderStatusHistoryRepository historyRepository,
        IPaymentRepository paymentRepository,
        IServicePackageRepository packageRepository,
        IMessageQueueService messageQueue,
        IPaymentService paymentService,
        IFusionCache cache,
        ILogger<OrderProcessingService> logger)
    {
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _paymentRepository = paymentRepository;
        _packageRepository = packageRepository;
        _messageQueue = messageQueue;
        _paymentService = paymentService;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order processing service started");

        await _messageQueue.SubscribeAsync<OrderQueueMessage>(
            "order.queue",
            async message => await ProcessOrderAsync(message, stoppingToken),
            stoppingToken);
    }

    private async Task ProcessOrderAsync(OrderQueueMessage message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing queued order: {OrderId}", message.OrderId);

            // Check if order was cancelled while in queue
            var isCancelled = await _cache.TryGetAsync<bool>($"order:cancelled:{message.OrderId}");
            if (isCancelled.HasValue && isCancelled.Value)
            {
                _logger.LogInformation("Order {OrderId} was cancelled, skipping processing", message.OrderId);
                await _cache.RemoveAsync($"order:cancelled:{message.OrderId}");
                return;
            }

            var package = await _cache.GetOrSetAsync(
                $"package:{message.ServicePackageId}",
                _ => _packageRepository.GetByIdAsync(message.ServicePackageId),
                new FusionCacheEntryOptions { Duration = TimeSpan.FromHours(1) });

            if (package == null)
            {
                _logger.LogError("Service package not found: {PackageId}", message.ServicePackageId);
                return;
            }

            var order = new ServiceOrder
            {
                Id = message.OrderId,
                OrderNo = message.OrderNo,
                CustomerId = message.CustomerId,
                ServicePackageId = message.ServicePackageId,
                PetId = message.PetId,
                ServiceDate = message.ServiceDate,
                Address = message.ServiceAddress,
                Price = package.Price,
                Status = OrderStatus.Pending,
                CustomerRemark = message.Remark,
                CreatedAt = message.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };

            var orderId = await _orderRepository.CreateAsync(order);

            var paymentIntent = await _paymentService.CreatePaymentIntentAsync(
                orderId,
                order.Price,
                "cny");

            var payment = new Payment
            {
                Id = YitIdHelper.NextId(),
                OrderId = orderId,
                Amount = order.Price,
                Currency = "CNY",
                PaymentIntentId = paymentIntent.PaymentIntentId!,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _paymentRepository.CreateAsync(payment);

            await _messageQueue.PublishAsync(
                "order.status_changed",
                new { OrderId = orderId, Status = OrderStatus.Pending.ToString(), Notes = "Order processed from queue" },
                cancellationToken);

            await _messageQueue.PublishAsync(
                "order.created",
                new OrderCreatedMessage { OrderId = orderId },
                cancellationToken);

            _logger.LogInformation("Order processed successfully: {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order: {OrderId}", message.OrderId);
            throw;
        }
    }

}

