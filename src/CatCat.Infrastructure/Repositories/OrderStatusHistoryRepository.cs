using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IOrderStatusHistoryRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:order_id}} {{orderby:created_at}}")]
    Task<List<OrderStatusHistory>> GetByOrderIdAsync(long orderId);

    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task<int> CreateAsync(OrderStatusHistory history);
}

[RepositoryFor(typeof(IOrderStatusHistoryRepository))]
public partial class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
{
    private readonly IDbConnection connection;

    public OrderStatusHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
