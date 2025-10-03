using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task CreateAsync(RefreshToken refreshToken);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:token}} AND revoked_at IS NULL")]
    Task<RefreshToken?> GetByTokenAsync(string token);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:user_id}} AND revoked_at IS NULL AND expires_at > @now {{orderby:created_at_desc}}")]
    Task<List<RefreshToken>> GetActiveByUserIdAsync(long userId, DateTime now);

    [Sqlx("{{update}} SET {{set:revoked_at,revoked_by_ip,reason_revoked}} WHERE {{where:token}}")]
    Task RevokeAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked);

    [Sqlx("{{update}} SET {{set:revoked_at,revoked_by_ip,reason_revoked,replaced_by_token}} WHERE {{where:token}}")]
    Task RevokeAndReplaceAsync(string token, DateTime revokedAt, string? revokedByIp, string? reasonRevoked, string? replacedByToken);

    [Sqlx("{{update}} SET {{set:revoked_at}} WHERE {{where:user_id}} AND revoked_at IS NULL")]
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
