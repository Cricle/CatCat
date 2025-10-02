using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.MessageQueue;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace CatCat.Infrastructure.Services;

public interface IReviewService
{
    Task<Result<long>> CreateReviewAsync(CreateReviewCommand command, CancellationToken cancellationToken = default);
    Task<Result<bool>> ReplyReviewAsync(long reviewId, string reply, CancellationToken cancellationToken = default);
    Task<Result<ReviewPagedResult>> GetServiceProviderReviewsAsync(
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
    /// Create review - Use NATS for async processing
    /// </summary>
    public async Task<Result<long>> CreateReviewAsync(CreateReviewCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Validate order status
        var order = await _orderRepository.GetByIdAsync(command.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Create review failed: Order not found. OrderId={OrderId}", command.OrderId);
            return Result.Failure<long>("Order not found");
        }

        if (order.Status != OrderStatus.Completed)
        {
            _logger.LogWarning("Create review failed: Order not completed. OrderId={OrderId}, Status={Status}",
                command.OrderId, order.Status);
            return Result.Failure<long>("Only completed orders can be reviewed");
        }

        if (order.CustomerId != command.CustomerId)
        {
            _logger.LogWarning("Create review failed: Not order owner. OrderId={OrderId}, CustomerId={CustomerId}, RequestUserId={RequestUserId}",
                command.OrderId, order.CustomerId, command.CustomerId);
            return Result.Failure<long>("You can only review your own orders");
        }

        // 2. Check if already reviewed
        var existingReview = await _reviewRepository.GetByOrderIdAsync(command.OrderId);
        if (existingReview != null)
        {
            _logger.LogWarning("Create review failed: Order already reviewed. OrderId={OrderId}", command.OrderId);
            return Result.Failure<long>("This order has already been reviewed");
        }

        // 3. Create review
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

        // 4. Publish review created event to NATS (async update statistics)
        await _messageQueue.PublishAsync("review.created", new ReviewCreatedEvent
        {
            ReviewId = reviewId,
            OrderId = command.OrderId,
            ServiceProviderId = order.ServiceProviderId.Value,
            Rating = command.Rating,
            CreatedAt = review.CreatedAt
        }, cancellationToken);

        _logger.LogInformation("Review created successfully: ReviewId={ReviewId}, OrderId={OrderId}", reviewId, command.OrderId);

        return Result.Success((long)reviewId);
    }

    /// <summary>
    /// Reply to review
    /// </summary>
    public async Task<Result<bool>> ReplyReviewAsync(long reviewId, string reply, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
        {
            _logger.LogWarning("Reply review failed: Review not found. ReviewId={ReviewId}", reviewId);
            return Result.Failure<bool>("Review not found");
        }

        if (!string.IsNullOrEmpty(review.Reply))
        {
            _logger.LogWarning("Reply review failed: Already replied. ReviewId={ReviewId}", reviewId);
            return Result.Failure<bool>("This review has already been replied to");
        }

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

            _logger.LogInformation("Review replied successfully: ReviewId={ReviewId}", reviewId);
            return Result.Success(true);
        }

        _logger.LogWarning("Reply review failed: Update failed. ReviewId={ReviewId}", reviewId);
        return Result.Failure<bool>("Failed to update review reply");
    }

    /// <summary>
    /// Get service provider reviews list
    /// </summary>
    public async Task<Result<ReviewPagedResult>> GetServiceProviderReviewsAsync(
        long serviceProviderId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var avgRating = await _reviewRepository.GetAverageRatingAsync(serviceProviderId);
        var offset = (page - 1) * pageSize;
        var items = await _reviewRepository.GetByServiceProviderIdPagedAsync(serviceProviderId, offset, pageSize);
        var total = await _reviewRepository.CountByServiceProviderIdAsync(serviceProviderId);

        _logger.LogInformation("Get reviews: ServiceProviderId={ServiceProviderId}, Total={Total}, AvgRating={AvgRating}",
            serviceProviderId, total, avgRating);

        return Result.Success(new ReviewPagedResult(items, total, (decimal)avgRating));
    }
}

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

