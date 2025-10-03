using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IUserRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<User?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:phone}}")]
    Task<User?> GetByPhoneAsync(string phone);

    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task<int> CreateAsync(User user);

    [Sqlx("{{update}} SET {{set:auto|exclude=Id}} WHERE {{where:id}}")]
    Task<int> UpdateAsync(User user);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} {{orderby:id}} {{limit:sqlite|offset=@offset|rows=@pageSize}}")]
    Task<List<User>> GetPagedAsync(int offset, int pageSize);

    [Sqlx("SELECT {{count:all}} FROM {{table}}")]
    Task<int> GetCountAsync();
}

[RepositoryFor(typeof(IUserRepository))]
public partial class UserRepository : IUserRepository
{
    private readonly IDbConnection connection;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
