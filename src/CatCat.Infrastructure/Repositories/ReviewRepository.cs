using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IReviewRepository
{
    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE id = @id")]
    Task<Review?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE order_id = @orderId")]
    Task<Review?> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE service_provider_id = @serviceProviderId ORDER BY created_at DESC LIMIT @pageSize OFFSET @offset")]
    Task<List<Review>> GetByServiceProviderIdPagedAsync(long serviceProviderId, int offset, int pageSize);

    [Sqlx("SELECT COUNT(*) FROM {{table}} WHERE service_provider_id = @serviceProviderId")]
    Task<int> CountByServiceProviderIdAsync(long serviceProviderId);

    [Sqlx("INSERT INTO {{table}} ({{columns --exclude Id}}) VALUES ({{values}})")]
    Task<int> CreateAsync(Review review);

    [Sqlx("UPDATE {{table}} SET reply = @reply, replied_at = @repliedAt, updated_at = @updatedAt WHERE id = @id")]
    Task<int> UpdateReplyAsync(long id, string reply, DateTime repliedAt, DateTime updatedAt);

    [Sqlx("SELECT AVG(rating) FROM {{table}} WHERE service_provider_id = @serviceProviderId")]
    Task<double> GetAverageRatingAsync(long serviceProviderId);
}

[RepositoryFor(typeof(IReviewRepository))]
public partial class ReviewRepository : IReviewRepository
{
    private readonly IDbConnection connection;

    public ReviewRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
