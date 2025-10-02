using CatCat.Domain.Entities;
using CatCat.Infrastructure.MessageQueue;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace CatCat.Core.Services;

public interface IReviewService
{
    Task<long> CreateReviewAsync(CreateReviewCommand command, CancellationToken cancellationToken = default);
    Task<bool> ReplyReviewAsync(long reviewId, string reply, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Review> Items, int Total, decimal AverageRating)> GetServiceProviderReviewsAsync(
        long serviceProviderId, int page, int pageSize, CancellationToken cancellationToken = default);
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IServiceOrderRepository _orderRepository;
    private readonly IMessageQueueService _messageQueue;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        IReviewRepository reviewRepository,
        IServiceOrderRepository orderRepository,
        IMessageQueueService messageQueue,
        ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _orderRepository = orderRepository;
        _messageQueue = messageQueue;
        _logger = logger;
    }

    /// <summary>
    /// 创建评价 - 使用NATS异步处理
    /// </summary>
    public async Task<long> CreateReviewAsync(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        // 1. 验证订单状态
        var order = await _orderRepository.GetByIdAsync(command.OrderId);
        if (order == null)
            throw new InvalidOperationException("订单不存在");

        if (order.Status != OrderStatus.Completed)
            throw new InvalidOperationException("只能对已完成的订单进行评价");

        if (order.CustomerId != command.CustomerId)
            throw new InvalidOperationException("只能评价自己的订单");

        // 2. 检查是否已评价
        var existingReview = await _reviewRepository.GetByOrderIdAsync(command.OrderId);
        if (existingReview != null)
            throw new InvalidOperationException("该订单已评价");

        // 3. 创建评价
        var review = new Review
        {
            OrderId = command.OrderId,
            CustomerId = command.CustomerId,
            ServiceProviderId = order.ServiceProviderId!.Value,
            Rating = command.Rating,
            Content = command.Content,
            PhotoUrls = command.PhotoUrls,
            CreatedAt = DateTime.UtcNow
        };

        var reviewId = await _reviewRepository.CreateAsync(review);

        // 4. 发布评价创建事件到NATS（异步更新统计数据）
        await _messageQueue.PublishAsync("review.created", new ReviewCreatedEvent
        {
            ReviewId = reviewId,
            OrderId = command.OrderId,
            ServiceProviderId = order.ServiceProviderId.Value,
            Rating = command.Rating,
            CreatedAt = review.CreatedAt
        }, cancellationToken);

        _logger.LogInformation("评价创建成功: ReviewId={ReviewId}, OrderId={OrderId}", reviewId, command.OrderId);

        return reviewId;
    }

    /// <summary>
    /// 回复评价
    /// </summary>
    public async Task<bool> ReplyReviewAsync(long reviewId, string reply, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            throw new InvalidOperationException("评价不存在");

        if (!string.IsNullOrEmpty(review.Reply))
            throw new InvalidOperationException("已回复过该评价");

        var affectedRows = await _reviewRepository.UpdateReplyAsync(reviewId, reply, DateTime.UtcNow, DateTime.UtcNow);

        if (affectedRows > 0)
        {
            await _messageQueue.PublishAsync("review.replied", new ReviewRepliedEvent
            {
                ReviewId = reviewId,
                ServiceProviderId = review.ServiceProviderId,
                Reply = reply,
                RepliedAt = DateTime.UtcNow
            }, cancellationToken);

            _logger.LogInformation("评价已回复: ReviewId={ReviewId}", reviewId);
        }

        return affectedRows > 0;
    }

    /// <summary>
    /// 获取服务人员的评价列表
    /// </summary>
    public async Task<(IEnumerable<Review> Items, int Total, decimal AverageRating)> GetServiceProviderReviewsAsync(
        long serviceProviderId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var avgRating = await _reviewRepository.GetAverageRatingAsync(serviceProviderId);
        var offset = (page - 1) * pageSize;
        var items = await _reviewRepository.GetByServiceProviderIdPagedAsync(serviceProviderId, offset, pageSize);
        var total = await _reviewRepository.CountByServiceProviderIdAsync(serviceProviderId);

        return (items, total, (decimal)avgRating);
    }
}

// 命令和事件定义
public record CreateReviewCommand(
    long OrderId,
    long CustomerId,
    int Rating,
    string? Content,
    string? PhotoUrls);

public record ReviewCreatedEvent
{
    public long ReviewId { get; init; }
    public long OrderId { get; init; }
    public long ServiceProviderId { get; init; }
    public int Rating { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ReviewRepliedEvent
{
    public long ReviewId { get; init; }
    public long ServiceProviderId { get; init; }
    public string Reply { get; init; } = string.Empty;
    public DateTime RepliedAt { get; init; }
}

