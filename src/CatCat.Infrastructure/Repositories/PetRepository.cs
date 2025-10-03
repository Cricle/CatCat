using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IPetRepository
{
    [Sqlx("SELECT {{column:auto}} FROM pets WHERE id = @id")]
    Task<Pet?> GetByIdAsync(long id);

    [Sqlx("SELECT {{column:auto}} FROM pets WHERE user_id = @userId ORDER BY created_at DESC")]
    Task<List<Pet>> GetByUserIdAsync(long userId);

    [Sqlx("INSERT INTO pets {{insert:auto}}")]
    Task<int> CreateAsync(Pet pet);

    [Sqlx("UPDATE pets {{update:auto}} WHERE id = @Id")]
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
