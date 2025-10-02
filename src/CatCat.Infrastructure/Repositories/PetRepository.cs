using CatCat.Domain.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IPetRepository
{
    [Sqlx("SELECT * FROM pets WHERE id = @id")]
    Task<Pet?> GetByIdAsync(long id);

    [Sqlx("SELECT * FROM pets WHERE user_id = @userId ORDER BY created_at DESC")]
    Task<List<Pet>> GetByUserIdAsync(long userId);

    [Sqlx("INSERT INTO pets (id, user_id, name, breed, age, gender, photo_url, health_info, created_at, updated_at) VALUES (@Id, @UserId, @Name, @Breed, @Age, @Gender, @PhotoUrl, @HealthInfo, @CreatedAt, @UpdatedAt)")]
    Task<int> CreateAsync(Pet pet);

    [Sqlx("UPDATE pets SET name = @Name, breed = @Breed, age = @Age, gender = @Gender, photo_url = @PhotoUrl, health_info = @HealthInfo, updated_at = @UpdatedAt WHERE id = @Id")]
    Task<int> UpdateAsync(Pet pet);

    [Sqlx("DELETE FROM pets WHERE id = @id")]
    Task<int> DeleteAsync(long id);
}

[RepositoryFor(typeof(IPetRepository))]
public partial class PetRepository : IPetRepository
{
    private readonly IDbConnection connection;

    public PetRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
