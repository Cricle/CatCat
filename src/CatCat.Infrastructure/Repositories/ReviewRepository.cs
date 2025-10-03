using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IReviewRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<Review?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:order_id}}")]
    Task<Review?> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:service_provider_id}} {{orderby:created_at_desc}} {{limit:postgresql|offset=@offset|rows=@pageSize}}")]
    Task<List<Review>> GetByServiceProviderIdPagedAsync(long serviceProviderId, int offset, int pageSize);

    [Sqlx("SELECT {{count:all}} FROM {{table}} WHERE {{where:service_provider_id}}")]
    Task<int> CountByServiceProviderIdAsync(long serviceProviderId);

    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task<int> CreateAsync(Review review);

    [Sqlx("{{update}} SET {{set:reply,replied_at,updated_at}} WHERE {{where:id}}")]
    Task<int> UpdateReplyAsync(long id, string reply, DateTime repliedAt, DateTime updatedAt);

    [Sqlx("SELECT {{avg:rating}} FROM {{table}} WHERE {{where:service_provider_id}}")]
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
