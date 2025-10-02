using CatCat.Domain.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CatCat.API.Endpoints;

public static class PetEndpoints
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pets")
            .WithTags("Pets")
            .RequireAuthorization()
            .WithOpenApi();

        // 获取我的宠物列表
        group.MapGet("/", async (
            ClaimsPrincipal user,
            IPetRepository petRepository) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var pets = await petRepository.GetByUserIdAsync(userId);

            return Results.Ok(new
            {
                items = pets,
                total = pets.Count()
            });
        })
        .WithName("GetMyPets")
        .WithSummary("获取我的猫猫档案列表");

        // 添加宠物
        group.MapPost("/", async (
            ClaimsPrincipal user,
            [FromBody] CreatePetRequest request,
            IPetRepository petRepository) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

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

            return Results.Ok(new
            {
                id = petId,
                message = "猫猫档案创建成功"
            });
        })
        .WithName("CreatePet")
        .WithSummary("创建猫猫档案");

        // 获取宠物详情
        group.MapGet("/{id}", async (
            long id,
            ClaimsPrincipal user,
            IPetRepository petRepository) =>
        {
            var pet = await petRepository.GetByIdAsync(id);
            if (pet == null)
            {
                return Results.NotFound(new { message = "猫猫档案不存在" });
            }

            return Results.Ok(pet);
        })
        .WithName("GetPetById")
        .WithSummary("获取猫猫档案详情");

        // 更新宠物信息
        group.MapPut("/{id}", async (
            long id,
            [FromBody] UpdatePetRequest request,
            ClaimsPrincipal user,
            IPetRepository petRepository) =>
        {
            var pet = await petRepository.GetByIdAsync(id);
            if (pet == null)
            {
                return Results.NotFound(new { message = "猫猫档案不存在" });
            }

            // 更新字段
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

            return Results.Ok(new { message = "猫猫档案更新成功" });
        })
        .WithName("UpdatePet")
        .WithSummary("更新猫猫档案");

        // 删除宠物
        group.MapDelete("/{id}", async (
            long id,
            ClaimsPrincipal user,
            IPetRepository petRepository) =>
        {
            await petRepository.DeleteAsync(id);
            return Results.Ok(new { message = "猫猫档案已删除" });
        })
        .WithName("DeletePet")
        .WithSummary("删除猫猫档案");
    }
}

public record CreatePetRequest(
    string Name,
    PetType Type,
    string? Breed,
    int Age,
    Gender Gender,
    string? Avatar,
    string? Character,
    string? DietaryHabits,
    string? HealthStatus,
    string? Remarks);

public record UpdatePetRequest(
    string? Name,
    string? Breed,
    int? Age,
    string? Avatar,
    string? Character,
    string? DietaryHabits,
    string? HealthStatus,
    string? Remarks);
