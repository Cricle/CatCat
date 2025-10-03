using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IPetRepository
{
    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE id = @id")]
    Task<Pet?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE user_id = @userId ORDER BY created_at DESC")]
    Task<List<Pet>> GetByUserIdAsync(long userId);

    [Sqlx("INSERT INTO {{table}} ({{columns --exclude Id}}) VALUES ({{values}})")]
    Task<int> CreateAsync(Pet pet);

    [Sqlx("UPDATE {{table}} SET {{set --exclude Id}} WHERE id = @Id")]
    Task<int> UpdateAsync(Pet pet);

    [Sqlx("DELETE FROM {{table}} WHERE id = @id")]
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
