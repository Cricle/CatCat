using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IServicePackageRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<ServicePackage?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:is_active}} {{orderby:price}}")]
    Task<List<ServicePackage>> GetActivePackagesAsync(bool isActive = true);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} {{orderby:id}} {{limit:sqlite|offset=@offset|rows=@pageSize}}")]
    Task<List<ServicePackage>> GetPagedAsync(int offset, int pageSize);

    [Sqlx("SELECT {{count:all}} FROM {{table}}")]
    Task<int> GetCountAsync();

    [Sqlx("SELECT id FROM service_packages")]
    Task<List<long>> GetAllIdsAsync();
}

[RepositoryFor(typeof(IServicePackageRepository))]
public partial class ServicePackageRepository : IServicePackageRepository
{
    private readonly IDbConnection connection;

    public ServicePackageRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
