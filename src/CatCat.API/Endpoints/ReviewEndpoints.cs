using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class ReviewEndpoints
{
    [RequiresUnreferencedCode("Uses JSON serialization which may require unreferenced code")]
    [RequiresDynamicCode("Uses JSON serialization which may require dynamic code")]
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews")
            .WithTags("Reviews");

        group.MapPost("/", async (ClaimsPrincipal user, [FromBody] CreateReviewRequest request, IReviewService reviewService) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            try
            {
                var command = new CreateReviewCommand(request.OrderId, userId, request.Rating, request.Content, request.PhotoUrls);
                var reviewId = await reviewService.CreateReviewAsync(command);
                return Results.Ok(new ReviewCreateResponse(reviewId, "评价成功"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new MessageResponse(ex.Message));
            }
        })
        .RequireAuthorization()
        .WithName("CreateReview")
        .WithSummary("创建评价");

        group.MapPost("/{id}/reply", async (long id, [FromBody] ReplyReviewRequest request, IReviewService reviewService) =>
        {
            try
            {
                await reviewService.ReplyReviewAsync(id, request.Reply);
                return Results.Ok(new MessageResponse("回复成功"));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new MessageResponse(ex.Message));
            }
        })
        .RequireAuthorization()
        .WithName("ReplyReview")
        .WithSummary("回复评价（服务人员）");

        group.MapGet("/service-provider/{serviceProviderId}", async (
            long serviceProviderId, IReviewService reviewService,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        {
            var (items, total, averageRating) = await reviewService.GetServiceProviderReviewsAsync(serviceProviderId, page, pageSize);
            return Results.Ok(new ReviewListResponse(items, total, averageRating, page, pageSize));
        })
        .WithName("GetServiceProviderReviews")
        .WithSummary("获取服务人员的评价列表");
    }
}
