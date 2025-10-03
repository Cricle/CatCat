using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IUserRepository
{
    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE id = @id")]
    Task<User?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE phone = @phone")]
    Task<User?> GetByPhoneAsync(string phone);

    [Sqlx("INSERT INTO {{table}} ({{columns --exclude Id}}) VALUES ({{values}})")]
    Task<int> CreateAsync(User user);

    [Sqlx("UPDATE {{table}} SET {{set --exclude Id}} WHERE id = @Id")]
    Task<int> UpdateAsync(User user);

    [Sqlx("SELECT {{columns}} FROM {{table}} ORDER BY id LIMIT @pageSize OFFSET @offset")]
    Task<List<User>> GetPagedAsync(int offset, int pageSize);

    [Sqlx("SELECT COUNT(*) FROM {{table}}")]
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
