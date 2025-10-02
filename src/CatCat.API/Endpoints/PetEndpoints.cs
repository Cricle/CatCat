using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Domain.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class PetEndpoints
{
    [RequiresUnreferencedCode("Uses JSON serialization which may require unreferenced code")]
    [RequiresDynamicCode("Uses JSON serialization which may require dynamic code")]
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IPetRepository petRepository) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var pets = await petRepository.GetByUserIdAsync(userId);
            return Results.Ok(new PetListResponse(pets, pets.Count));
        })
        .WithName("GetMyPets")
        .WithSummary("获取我的猫猫档案列表");

        group.MapPost("/", async (ClaimsPrincipal user, [FromBody] CreatePetRequest request, IPetRepository petRepository) =>
        {
            if (!user.TryGetUserId(out var userId))
                return Results.Unauthorized();

            var pet = new Pet
            {
                UserId = userId,
                Name = request.Name,
                Type = request.Type,
                Breed = request.Breed,
                Age = request.Age,
                Gender = request.Gender,
                Avatar = request.Avatar,
                Character = request.Character,
                DietaryHabits = request.DietaryHabits,
                HealthStatus = request.HealthStatus,
                Remarks = request.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            var petId = await petRepository.CreateAsync(pet);
            return Results.Ok(new PetCreateResponse(petId, "猫猫档案创建成功"));
        })
        .WithName("CreatePet")
        .WithSummary("创建猫猫档案");

        group.MapGet("/{id}", async (long id, IPetRepository petRepository) =>
        {
            var pet = await petRepository.GetByIdAsync(id);
            return pet == null
                ? Results.NotFound(ApiResult.NotFound("猫猫档案不存在"))
                : Results.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("获取猫猫档案详情");

        group.MapPut("/{id}", async (long id, [FromBody] UpdatePetRequest request, IPetRepository petRepository) =>
        {
            var pet = await petRepository.GetByIdAsync(id);
            if (pet == null)
                return Results.NotFound(ApiResult.NotFound("猫猫档案不存在"));

            if (request.Name != null) pet.Name = request.Name;
            if (request.Breed != null) pet.Breed = request.Breed;
            if (request.Age.HasValue) pet.Age = request.Age.Value;
            if (request.Avatar != null) pet.Avatar = request.Avatar;
            if (request.Character != null) pet.Character = request.Character;
            if (request.DietaryHabits != null) pet.DietaryHabits = request.DietaryHabits;
            if (request.HealthStatus != null) pet.HealthStatus = request.HealthStatus;
            if (request.Remarks != null) pet.Remarks = request.Remarks;
            pet.UpdatedAt = DateTime.UtcNow;

            await petRepository.UpdateAsync(pet);
            return Results.Ok(new MessageResponse("猫猫档案更新成功"));
        })
        .WithName("UpdatePet")
        .WithSummary("更新猫猫档案");

        group.MapDelete("/{id}", async (long id, IPetRepository petRepository) =>
        {
            await petRepository.DeleteAsync(id);
            return Results.Ok(new MessageResponse("猫猫档案已删除"));
        })
        .WithName("DeletePet")
        .WithSummary("删除猫猫档案");
    }
}
