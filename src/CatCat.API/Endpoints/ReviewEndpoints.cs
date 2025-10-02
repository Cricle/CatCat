using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews")
            .WithTags("Reviews");

        group.MapPost("/", async (ClaimsPrincipal user, [FromBody] CreateReviewRequest request, IReviewService reviewService) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var command = new CreateReviewCommand(request.OrderId, userId, request.Rating, request.Content, request.PhotoUrls);
            var result = await reviewService.CreateReviewAsync(command);

            return result.IsSuccess
                ? Results.Ok(new ReviewCreateResponse(result.Value, "Review created successfully"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .WithName("CreateReview")
        .WithSummary("Create review");

        group.MapPost("/{id}/reply", async (long id, [FromBody] ReplyReviewRequest request, IReviewService reviewService) =>
        {
            var result = await reviewService.ReplyReviewAsync(id, request.Reply);

            return result.IsSuccess
                ? Results.Ok(new MessageResponse("Reply sent successfully"))
                : Results.BadRequest(ApiResult.Fail(result.Error!));
        })
        .RequireAuthorization()
        .WithName("ReplyReview")
        .WithSummary("Reply to review (Service provider)");

        group.MapGet("/service-provider/{serviceProviderId}", async (
            long serviceProviderId, IReviewService reviewService,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10) =>
        {
            var result = await reviewService.GetServiceProviderReviewsAsync(serviceProviderId, page, pageSize);

            if (!result.IsSuccess)
                return Results.BadRequest(ApiResult.Fail(result.Error!));

            var (items, total, averageRating) = result.Value;
            return Results.Ok(new ReviewListResponse(items, total, averageRating, page, pageSize));
        })
        .WithName("GetServiceProviderReviews")
        .WithSummary("Get service provider reviews list");
    }
}
