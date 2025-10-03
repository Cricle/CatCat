using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    [Sqlx("INSERT INTO refresh_tokens {{column:auto}} VALUES {{value:auto}}")]
    Task CreateAsync(RefreshToken refreshToken);

    [Sqlx("SELECT {{column:auto}} FROM refresh_tokens WHERE token = @token AND revoked_at IS NULL")]
    Task<RefreshToken?> GetByTokenAsync(string token);

    [Sqlx("SELECT {{column:auto}} FROM refresh_tokens WHERE user_id = @userId AND revoked_at IS NULL AND expires_at > @now ORDER BY created_at DESC")]
    Task<List<RefreshToken>> GetActiveByUserIdAsync(long userId, DateTime now);

    [Sqlx("UPDATE refresh_tokens SET {{set:auto}} WHERE token = @token")]
    Task RevokeAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked);

    [Sqlx("UPDATE refresh_tokens SET {{set:auto}} WHERE token = @token")]
    Task RevokeAndReplaceAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked, string? replacedByToken);

    [Sqlx("UPDATE refresh_tokens SET {{set:auto}} WHERE user_id = @userId AND revoked_at IS NULL")]
    Task RevokeAllByUserIdAsync(long userId, DateTime revokedAt);
}

[RepositoryFor(typeof(IRefreshTokenRepository))]
public partial class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnection connection;

    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
