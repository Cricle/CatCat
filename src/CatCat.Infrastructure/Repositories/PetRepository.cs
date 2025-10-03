using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IPetRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<Pet?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:user_id}} {{orderby:created_at_desc}}")]
    Task<List<Pet>> GetByUserIdAsync(long userId);

    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task<int> CreateAsync(Pet pet);

    [Sqlx("{{update}} SET {{set:auto|exclude=Id}} WHERE {{where:id}}")]
    Task<int> UpdateAsync(Pet pet);

    [Sqlx("{{delete}} WHERE {{where:id}}")]
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
