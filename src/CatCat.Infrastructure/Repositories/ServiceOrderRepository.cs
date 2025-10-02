using CatCat.Domain.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IServiceOrderRepository
{
    [Sqlx("SELECT * FROM service_orders WHERE id = @id")]
    Task<ServiceOrder?> GetByIdAsync(long id);

    [Sqlx("SELECT * FROM service_orders WHERE order_no = @orderNo")]
    Task<ServiceOrder?> GetByOrderNoAsync(string orderNo);

    [Sqlx("INSERT INTO service_orders (id, order_no, customer_id, service_provider_id, pet_id, package_id, scheduled_time, address, total_price, status, notes, created_at, updated_at) VALUES (@Id, @OrderNo, @CustomerId, @ServiceProviderId, @PetId, @PackageId, @ScheduledTime, @Address, @TotalPrice, @Status, @Notes, @CreatedAt, @UpdatedAt)")]
    Task<int> CreateAsync(ServiceOrder order);

    [Sqlx("UPDATE service_orders SET status = @Status, notes = @Notes, updated_at = @UpdatedAt WHERE id = @Id")]
    Task<int> UpdateAsync(ServiceOrder order);

    [Sqlx("UPDATE service_orders SET status = @status, updated_at = @updatedAt WHERE id = @id")]
    Task<int> UpdateStatusAsync(long id, string status, DateTime updatedAt);

    [Sqlx("SELECT * FROM service_orders WHERE customer_id = @customerId ORDER BY created_at DESC LIMIT @pageSize OFFSET @offset")]
    Task<List<ServiceOrder>> GetByCustomerIdPagedAsync(long customerId, int offset, int pageSize);

    [Sqlx("SELECT * FROM service_orders WHERE customer_id = @customerId AND status = @status ORDER BY created_at DESC LIMIT @pageSize OFFSET @offset")]
    Task<List<ServiceOrder>> GetByCustomerIdAndStatusPagedAsync(long customerId, string status, int offset, int pageSize);

    [Sqlx("SELECT COUNT(*) FROM service_orders WHERE customer_id = @customerId")]
    Task<int> CountByCustomerIdAsync(long customerId);

    [Sqlx("SELECT COUNT(*) FROM service_orders WHERE customer_id = @customerId AND status = @status")]
    Task<int> CountByCustomerIdAndStatusAsync(long customerId, string status);

    [Sqlx("SELECT * FROM service_orders WHERE service_provider_id = @serviceProviderId ORDER BY created_at DESC LIMIT @pageSize OFFSET @offset")]
    Task<List<ServiceOrder>> GetByServiceProviderIdPagedAsync(long serviceProviderId, int offset, int pageSize);

    [Sqlx("SELECT * FROM service_orders WHERE service_provider_id = @serviceProviderId AND status = @status ORDER BY created_at DESC LIMIT @pageSize OFFSET @offset")]
    Task<List<ServiceOrder>> GetByServiceProviderIdAndStatusPagedAsync(long serviceProviderId, string status, int offset, int pageSize);

    [Sqlx("SELECT COUNT(*) FROM service_orders WHERE service_provider_id = @serviceProviderId")]
    Task<int> CountByServiceProviderIdAsync(long serviceProviderId);

    [Sqlx("SELECT COUNT(*) FROM service_orders WHERE service_provider_id = @serviceProviderId AND status = @status")]
    Task<int> CountByServiceProviderIdAndStatusAsync(long serviceProviderId, string status);
}

[RepositoryFor(typeof(IServiceOrderRepository))]
public partial class ServiceOrderRepository : IServiceOrderRepository
{
    private readonly IDbConnection connection;

    public ServiceOrderRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
