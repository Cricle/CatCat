using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IOrderStatusHistoryRepository
{
    [Sqlx("SELECT * FROM order_status_history WHERE order_id = @orderId ORDER BY created_at")]
    Task<List<OrderStatusHistory>> GetByOrderIdAsync(long orderId);

    [Sqlx("INSERT INTO order_status_history (id, order_id, status, notes, created_at) VALUES (@Id, @OrderId, @Status, @Notes, @CreatedAt)")]
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
