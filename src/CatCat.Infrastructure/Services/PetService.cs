using CatCat.Infrastructure.BloomFilter;
using CatCat.Infrastructure.Common;
using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;
using ZiggyCreatures.Caching.Fusion;

namespace CatCat.Infrastructure.Services;

public interface IPetService
{
    Task<Result<Pet>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Result<List<Pet>>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);
    Task<Result<long>> CreateAsync(CreatePetCommand command, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(UpdatePetCommand command, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(long id, long userId, CancellationToken cancellationToken = default);
}

public record CreatePetCommand(
    long UserId,
    string Name,
    int Type,
    string? Breed,
    int Age,
    int Gender,
    string? Avatar,
    string? Character,
    string? DietaryHabits,
    string? HealthStatus,
    string? Remarks);

public record UpdatePetCommand(
    long Id,
    long UserId,
    string Name,
    int Type,
    string? Breed,
    int Age,
    int Gender,
    string? Avatar,
    string? Character,
    string? DietaryHabits,
    string? HealthStatus,
    string? Remarks);

public class PetService(
    IPetRepository repository,
    IFusionCache cache,
    IBloomFilterService bloomFilter,
    ILogger<PetService> logger) : IPetService
{
    // Cache keys
    private const string PetCacheKeyPrefix = "pet:";
    private const string UserPetsCacheKeyPrefix = "user:pets:";

    // Cache durations (pet info changes occasionally)
    private static readonly TimeSpan PetCacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan UserPetsCacheDuration = TimeSpan.FromMinutes(15);

    public async Task<Result<Pet>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        // Bloom filter: quickly reject non-existent IDs (prevent cache penetration)
        if (!bloomFilter.MightContainPet(id))
        {
            logger.LogDebug("Pet {PetId} blocked by Bloom Filter (not exist)", id);
            return Result.Failure<Pet>("Pet not found");
        }

        var pet = await cache.GetOrSetAsync<Pet?>(
            $"{PetCacheKeyPrefix}{id}",
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for pet {PetId}, fetching from DB", id);
                return await repository.GetByIdAsync(id);
            },
            options => options.SetDuration(PetCacheDuration),
            cancellationToken);

        return pet != null
            ? Result.Success(pet)
            : Result.Failure<Pet>("Pet not found");
    }

    public async Task<Result<List<Pet>>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var pets = await cache.GetOrSetAsync<List<Pet>>(
            $"{UserPetsCacheKeyPrefix}{userId}",
            async (ctx, ct) =>
            {
                logger.LogDebug("Cache miss for user {UserId} pets, fetching from DB", userId);
                return await repository.GetByUserIdAsync(userId);
            },
            options => options.SetDuration(UserPetsCacheDuration),
            cancellationToken);

        return Result.Success(pets);
    }

    public async Task<Result<long>> CreateAsync(CreatePetCommand command, CancellationToken cancellationToken = default)
    {
        var pet = new Pet
        {
            Id = YitIdHelper.NextId(),
            UserId = command.UserId,
            Name = command.Name,
            Type = (PetType)command.Type,
            Breed = command.Breed,
            Age = command.Age,
            Gender = (Gender)command.Gender,
            Avatar = command.Avatar,
            Character = command.Character,
            DietaryHabits = command.DietaryHabits,
            HealthStatus = command.HealthStatus,
            Remarks = command.Remarks,
            CreatedAt = DateTime.UtcNow
        };

        var affectedRows = await repository.CreateAsync(pet);
        if (affectedRows > 0)
        {
            // Add to Bloom Filter
            bloomFilter.AddPet(pet.Id);

            // Invalidate user pets cache
            await cache.RemoveAsync($"{UserPetsCacheKeyPrefix}{command.UserId}");

            logger.LogInformation("Pet {PetId} created for user {UserId}", pet.Id, command.UserId);
            return Result.Success(pet.Id);
        }

        return Result.Failure<long>("Create pet failed");
    }

    public async Task<Result> UpdateAsync(UpdatePetCommand command, CancellationToken cancellationToken = default)
    {
        var pet = await repository.GetByIdAsync(command.Id);
        if (pet == null)
            return Result.Failure("Pet not found");

        if (pet.UserId != command.UserId)
            return Result.Failure("Unauthorized");

        pet.Name = command.Name;
        pet.Type = (PetType)command.Type;
        pet.Breed = command.Breed;
        pet.Age = command.Age;
        pet.Gender = (Gender)command.Gender;
        pet.Avatar = command.Avatar;
        pet.Character = command.Character;
        pet.DietaryHabits = command.DietaryHabits;
        pet.HealthStatus = command.HealthStatus;
        pet.Remarks = command.Remarks;
        pet.UpdatedAt = DateTime.UtcNow;

        var affectedRows = await repository.UpdateAsync(pet);
        if (affectedRows > 0)
        {
            // Invalidate caches
            await cache.RemoveAsync($"{PetCacheKeyPrefix}{command.Id}");
            await cache.RemoveAsync($"{UserPetsCacheKeyPrefix}{command.UserId}");

            logger.LogInformation("Pet {PetId} updated", command.Id);
            return Result.Success();
        }

        return Result.Failure("Update pet failed");
    }

    public async Task<Result> DeleteAsync(long id, long userId, CancellationToken cancellationToken = default)
    {
        var pet = await repository.GetByIdAsync(id);
        if (pet == null)
            return Result.Failure("Pet not found");

        if (pet.UserId != userId)
            return Result.Failure("Unauthorized");

        var affectedRows = await repository.DeleteAsync(id);
        if (affectedRows > 0)
        {
            // Invalidate caches
            await cache.RemoveAsync($"{PetCacheKeyPrefix}{id}");
            await cache.RemoveAsync($"{UserPetsCacheKeyPrefix}{userId}");

            logger.LogInformation("Pet {PetId} deleted", id);
            return Result.Success();
        }

        return Result.Failure("Delete pet failed");
    }
}

