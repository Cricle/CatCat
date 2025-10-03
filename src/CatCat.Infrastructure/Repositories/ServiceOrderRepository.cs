using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IServiceOrderRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<ServiceOrder?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:order_no}}")]
    Task<ServiceOrder?> GetByOrderNoAsync(string orderNo);

    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task<int> CreateAsync(ServiceOrder order);

    [Sqlx("{{update}} SET {{set:auto|exclude=Id}} WHERE {{where:id}}")]
    Task<int> UpdateAsync(ServiceOrder order);

    [Sqlx("{{update}} SET {{set:status,updated_at}} WHERE {{where:id}}")]
    Task<int> UpdateStatusAsync(long id, string status, DateTime updatedAt);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:customer_id}} {{orderby:created_at_desc}} {{limit:postgresql|offset=@offset|rows=@pageSize}}")]
    Task<List<ServiceOrder>> GetByCustomerIdPagedAsync(long customerId, int offset, int pageSize);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:customer_id}} AND {{where:status}} {{orderby:created_at_desc}} {{limit:postgresql|offset=@offset|rows=@pageSize}}")]
    Task<List<ServiceOrder>> GetByCustomerIdAndStatusPagedAsync(long customerId, string status, int offset, int pageSize);

    [Sqlx("SELECT {{count:all}} FROM {{table}} WHERE {{where:customer_id}}")]
    Task<int> CountByCustomerIdAsync(long customerId);

    [Sqlx("SELECT {{count:all}} FROM {{table}} WHERE {{where:customer_id}} AND {{where:status}}")]
    Task<int> CountByCustomerIdAndStatusAsync(long customerId, string status);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:service_provider_id}} {{orderby:created_at_desc}} {{limit:postgresql|offset=@offset|rows=@pageSize}}")]
    Task<List<ServiceOrder>> GetByServiceProviderIdPagedAsync(long serviceProviderId, int offset, int pageSize);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:service_provider_id}} AND {{where:status}} {{orderby:created_at_desc}} {{limit:postgresql|offset=@offset|rows=@pageSize}}")]
    Task<List<ServiceOrder>> GetByServiceProviderIdAndStatusPagedAsync(long serviceProviderId, string status, int offset, int pageSize);

    [Sqlx("SELECT {{count:all}} FROM {{table}} WHERE {{where:service_provider_id}}")]
    Task<int> CountByServiceProviderIdAsync(long serviceProviderId);

    [Sqlx("SELECT {{count:all}} FROM {{table}} WHERE {{where:service_provider_id}} AND {{where:status}}")]
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
