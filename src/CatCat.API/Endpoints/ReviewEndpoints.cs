using CatCat.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews")
            .WithTags("Reviews")
            .WithOpenApi();

        // 创建评价
        group.MapPost("/", async (
            ClaimsPrincipal user,
            [FromBody] CreateReviewRequest request,
            IReviewService reviewService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            try
            {
                var command = new CreateReviewCommand(
                    request.OrderId,
                    userId,
                    request.Rating,
                    request.Content,
                    request.PhotoUrls);

                var reviewId = await reviewService.CreateReviewAsync(command);

                return Results.Ok(new
                {
                    reviewId,
                    message = "评价成功"
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithName("CreateReview")
        .WithSummary("创建评价");

        // 回复评价（服务人员）
        group.MapPost("/{id}/reply", async (
            long id,
            [FromBody] ReplyReviewRequest request,
            ClaimsPrincipal user,
            IReviewService reviewService) =>
        {
            try
            {
                await reviewService.ReplyReviewAsync(id, request.Reply);
                return Results.Ok(new { message = "回复成功" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithName("ReplyReview")
        .WithSummary("回复评价（服务人员）");

        // 获取服务人员的评价列表（公开）
        group.MapGet("/service-provider/{serviceProviderId}", async (
            long serviceProviderId,
            IReviewService reviewService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            var (items, total, averageRating) = await reviewService.GetServiceProviderReviewsAsync(
                serviceProviderId, page, pageSize);

            return Results.Ok(new
            {
                items,
                total,
                averageRating,
                page,
                pageSize
            });
        })
        .WithName("GetServiceProviderReviews")
        .WithSummary("获取服务人员的评价列表");
    }
}

public record CreateReviewRequest(
    long OrderId,
    int Rating,
    string? Content,
    string? PhotoUrls);

public record ReplyReviewRequest(string Reply);

