using CatCat.Infrastructure.Database;
using CatCat.Infrastructure.Entities;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IServiceProgressRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:order_id}} ORDER BY created_at DESC")]
    Task<List<ServiceProgress>> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<ServiceProgress?> GetByIdAsync(long id);

    [Sqlx("INSERT INTO {{table}} ({{columns --exclude Id}}) VALUES ({{values}}) RETURNING id")]
    Task<long> CreateAsync(ServiceProgress progress);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:order_id}} ORDER BY created_at DESC {{limit:sqlite|rows=1}}")]
    Task<ServiceProgress?> GetLatestByOrderIdAsync(long orderId);
}

[RepositoryFor(typeof(IServiceProgressRepository))]
public partial class ServiceProgressRepository : IServiceProgressRepository
{
    private readonly IDbConnection connection;

    public ServiceProgressRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}

