using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    [Sqlx("INSERT INTO refresh_tokens (id, user_id, token, expires_at, created_at, created_by_ip) VALUES (@Id, @UserId, @Token, @ExpiresAt, @CreatedAt, @CreatedByIp)")]
    Task CreateAsync(RefreshToken refreshToken);

    [Sqlx("SELECT * FROM refresh_tokens WHERE token = @token AND revoked_at IS NULL")]
    Task<RefreshToken?> GetByTokenAsync(string token);

    [Sqlx("SELECT * FROM refresh_tokens WHERE user_id = @userId AND revoked_at IS NULL AND expires_at > @now ORDER BY created_at DESC")]
    Task<List<RefreshToken>> GetActiveByUserIdAsync(long userId, DateTime now);

    [Sqlx("UPDATE refresh_tokens SET revoked_at = @revokedAt, revoked_by_ip = @revokedByIp, reason_revoked = @reasonRevoked WHERE token = @token")]
    Task RevokeAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked);

    [Sqlx("UPDATE refresh_tokens SET revoked_at = @revokedAt, revoked_by_ip = @revokedByIp, reason_revoked = @reasonRevoked, replaced_by_token = @replacedByToken WHERE token = @token")]
    Task RevokeAndReplaceAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked, string? replacedByToken);

    [Sqlx("UPDATE refresh_tokens SET revoked_at = @revokedAt, reason_revoked = 'Logout' WHERE user_id = @userId AND revoked_at IS NULL")]
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
