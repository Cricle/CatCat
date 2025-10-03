using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    [Sqlx("INSERT INTO {{table}} ({{columns --exclude Id}}) VALUES ({{values}})")]
    Task CreateAsync(RefreshToken refreshToken);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE token = @token AND revoked_at IS NULL")]
    Task<RefreshToken?> GetByTokenAsync(string token);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE user_id = @userId AND revoked_at IS NULL AND expires_at > @now ORDER BY created_at DESC")]
    Task<List<RefreshToken>> GetActiveByUserIdAsync(long userId, DateTime now);

    [Sqlx("UPDATE {{table}} SET {{set --exclude Id UserId Token CreatedAt CreatedByIp}} WHERE token = @token")]
    Task RevokeAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked);

    [Sqlx("UPDATE {{table}} SET {{set --exclude Id UserId Token CreatedAt CreatedByIp}} WHERE token = @token")]
    Task RevokeAndReplaceAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked, string? replacedByToken);

    [Sqlx("UPDATE {{table}} SET revoked_at = @revokedAt, reason_revoked = 'Logout' WHERE user_id = @userId AND revoked_at IS NULL")]
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
