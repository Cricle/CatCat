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

        // Simplified property updates using null-coalescing
        pet.Name = request.Name ?? pet.Name;
        pet.Breed = request.Breed ?? pet.Breed;
        pet.Age = request.Age ?? pet.Age;
        pet.Avatar = request.Avatar ?? pet.Avatar;
        pet.Character = request.Character ?? pet.Character;
        pet.DietaryHabits = request.DietaryHabits ?? pet.DietaryHabits;
        pet.HealthStatus = request.HealthStatus ?? pet.HealthStatus;
        pet.Remarks = request.Remarks ?? pet.Remarks;
        pet.FoodLocationImage = request.FoodLocationImage ?? pet.FoodLocationImage;
        pet.FoodLocationDesc = request.FoodLocationDesc ?? pet.FoodLocationDesc;
        pet.WaterLocationImage = request.WaterLocationImage ?? pet.WaterLocationImage;
        pet.WaterLocationDesc = request.WaterLocationDesc ?? pet.WaterLocationDesc;
        pet.LitterBoxLocationImage = request.LitterBoxLocationImage ?? pet.LitterBoxLocationImage;
        pet.LitterBoxLocationDesc = request.LitterBoxLocationDesc ?? pet.LitterBoxLocationDesc;
        pet.CleaningSuppliesImage = request.CleaningSuppliesImage ?? pet.CleaningSuppliesImage;
        pet.CleaningSuppliesDesc = request.CleaningSuppliesDesc ?? pet.CleaningSuppliesDesc;
        pet.NeedsWaterRefill = request.NeedsWaterRefill ?? pet.NeedsWaterRefill;
        pet.SpecialInstructions = request.SpecialInstructions ?? pet.SpecialInstructions;
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
