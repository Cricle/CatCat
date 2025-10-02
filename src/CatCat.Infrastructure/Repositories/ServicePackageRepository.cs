using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;

namespace CatCat.Infrastructure.Repositories;

public interface IServicePackageRepository
{
    [Sqlx("SELECT * FROM service_packages WHERE id = @id")]
    Task<ServicePackage?> GetByIdAsync(long id);

    [Sqlx("SELECT * FROM service_packages WHERE is_active = @isActive ORDER BY price")]
    Task<List<ServicePackage>> GetActivePackagesAsync(bool isActive = true);

    [Sqlx("SELECT * FROM service_packages ORDER BY id LIMIT @pageSize OFFSET @offset")]
    Task<List<ServicePackage>> GetPagedAsync(int offset, int pageSize);

    [Sqlx("SELECT COUNT(*) FROM service_packages")]
    Task<int> GetCountAsync();
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
