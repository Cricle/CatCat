using CatCat.API.Extensions;
using CatCat.API.Models;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class PetEndpoints
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets")
            .RequireAuthorization();

        group.MapGet("/", GetMyPets)
            .WithName("GetMyPets")
            .WithSummary("Get my pet profiles");

        group.MapPost("/", CreatePet)
            .WithName("CreatePet")
            .WithSummary("Create pet profile");

        group.MapGet("/{id}", GetPetById)
            .WithName("GetPetById")
            .WithSummary("Get pet profile by ID");

        group.MapPut("/{id}", UpdatePet)
            .WithName("UpdatePet")
            .WithSummary("Update pet profile");

        group.MapDelete("/{id}", DeletePet)
            .WithName("DeletePet")
            .WithSummary("Delete pet profile");
    }

    private static async Task<IResult> GetMyPets(ClaimsPrincipal user, IPetRepository petRepository)
    {
        if (!user.TryGetUserId(out var userId))
            return Results.Unauthorized();

        var pets = await petRepository.GetByUserIdAsync(userId);
        return Results.Ok(new PetListResponse(pets, pets.Count));
    }

    private static async Task<IResult> CreatePet(
        ClaimsPrincipal user,
        [FromBody] CreatePetRequest request,
        IPetRepository petRepository)
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
            FoodLocationImage = request.FoodLocationImage,
            FoodLocationDesc = request.FoodLocationDesc,
            WaterLocationImage = request.WaterLocationImage,
            WaterLocationDesc = request.WaterLocationDesc,
            LitterBoxLocationImage = request.LitterBoxLocationImage,
            LitterBoxLocationDesc = request.LitterBoxLocationDesc,
            CleaningSuppliesImage = request.CleaningSuppliesImage,
            CleaningSuppliesDesc = request.CleaningSuppliesDesc,
            NeedsWaterRefill = request.NeedsWaterRefill,
            SpecialInstructions = request.SpecialInstructions,
            CreatedAt = DateTime.UtcNow
        };

        var petId = await petRepository.CreateAsync(pet);
        return Results.Ok(new PetCreateResponse(petId, "Pet profile created successfully"));
    }

    private static async Task<IResult> GetPetById(long id, IPetRepository petRepository)
    {
        var pet = await petRepository.GetByIdAsync(id);
        return pet == null
            ? Results.NotFound(ApiResult.NotFound("Pet profile not found"))
            : Results.Ok(pet);
    }

    private static async Task<IResult> UpdatePet(
        long id,
        [FromBody] UpdatePetRequest request,
        IPetRepository petRepository)
    {
        var pet = await petRepository.GetByIdAsync(id);
        if (pet == null)
            return Results.NotFound(ApiResult.NotFound("Pet profile not found"));

        if (request.Name != null) pet.Name = request.Name;
        if (request.Breed != null) pet.Breed = request.Breed;
        if (request.Age.HasValue) pet.Age = request.Age.Value;
        if (request.Avatar != null) pet.Avatar = request.Avatar;
        if (request.Character != null) pet.Character = request.Character;
        if (request.DietaryHabits != null) pet.DietaryHabits = request.DietaryHabits;
        if (request.HealthStatus != null) pet.HealthStatus = request.HealthStatus;
        if (request.Remarks != null) pet.Remarks = request.Remarks;
        if (request.FoodLocationImage != null) pet.FoodLocationImage = request.FoodLocationImage;
        if (request.FoodLocationDesc != null) pet.FoodLocationDesc = request.FoodLocationDesc;
        if (request.WaterLocationImage != null) pet.WaterLocationImage = request.WaterLocationImage;
        if (request.WaterLocationDesc != null) pet.WaterLocationDesc = request.WaterLocationDesc;
        if (request.LitterBoxLocationImage != null) pet.LitterBoxLocationImage = request.LitterBoxLocationImage;
        if (request.LitterBoxLocationDesc != null) pet.LitterBoxLocationDesc = request.LitterBoxLocationDesc;
        if (request.CleaningSuppliesImage != null) pet.CleaningSuppliesImage = request.CleaningSuppliesImage;
        if (request.CleaningSuppliesDesc != null) pet.CleaningSuppliesDesc = request.CleaningSuppliesDesc;
        if (request.NeedsWaterRefill.HasValue) pet.NeedsWaterRefill = request.NeedsWaterRefill.Value;
        if (request.SpecialInstructions != null) pet.SpecialInstructions = request.SpecialInstructions;
        pet.UpdatedAt = DateTime.UtcNow;

        await petRepository.UpdateAsync(pet);
        return Results.Ok(new MessageResponse("Pet profile updated successfully"));
    }

    private static async Task<IResult> DeletePet(long id, IPetRepository petRepository)
    {
        await petRepository.DeleteAsync(id);
        return Results.Ok(new MessageResponse("Pet profile deleted successfully"));
    }
}
