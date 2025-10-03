using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IUserRepository
{
    [Sqlx("SELECT {{column:auto}} FROM users WHERE id = @id")]
    Task<User?> GetByIdAsync(long id);

    [Sqlx("SELECT {{column:auto}} FROM users WHERE phone = @phone")]
    Task<User?> GetByPhoneAsync(string phone);

    [Sqlx("INSERT INTO users {{column:auto}} VALUES {{value:auto}}")]
    Task<int> CreateAsync(User user);

    [Sqlx("UPDATE users SET {{set:auto}} WHERE id = @Id")]
    Task<int> UpdateAsync(User user);

    [Sqlx("SELECT {{column:auto}} FROM users ORDER BY id LIMIT @pageSize OFFSET @offset")]
    Task<List<User>> GetPagedAsync(int offset, int pageSize);

    [Sqlx("SELECT COUNT(*) FROM users")]
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
