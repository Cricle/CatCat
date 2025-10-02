using CatCat.Domain.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IUserRepository
{
    [Sqlx("SELECT * FROM users WHERE id = @id")]
    Task<User?> GetByIdAsync(long id);

    [Sqlx("SELECT * FROM users WHERE phone = @phone")]
    Task<User?> GetByPhoneAsync(string phone);

    [Sqlx("INSERT INTO users (id, phone, password_hash, role, nickname, avatar_url, created_at, updated_at) VALUES (@Id, @Phone, @PasswordHash, @Role, @Nickname, @AvatarUrl, @CreatedAt, @UpdatedAt)")]
    Task<int> CreateAsync(User user);

    [Sqlx("UPDATE users SET phone = @Phone, password_hash = @PasswordHash, role = @Role, nickname = @Nickname, avatar_url = @AvatarUrl, updated_at = @UpdatedAt WHERE id = @Id")]
    Task<int> UpdateAsync(User user);

    [Sqlx("SELECT * FROM users ORDER BY id LIMIT @pageSize OFFSET @offset")]
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
