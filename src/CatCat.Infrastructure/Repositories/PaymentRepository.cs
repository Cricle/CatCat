using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;
using PaymentEntity = CatCat.Infrastructure.Entities.Payment;

namespace CatCat.Infrastructure.Repositories;

public interface IPaymentRepository
{
    [Sqlx("SELECT {{column:auto}} FROM payments WHERE id = @id")]
    Task<PaymentEntity?> GetByIdAsync(long id);

    [Sqlx("SELECT {{column:auto}} FROM payments WHERE order_id = @orderId")]
    Task<PaymentEntity?> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT {{column:auto}} FROM payments WHERE payment_intent_id = @paymentIntentId")]
    Task<PaymentEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);

    [Sqlx("INSERT INTO payments {{column:auto}} VALUES {{value:auto}}")]
    Task<int> CreateAsync(PaymentEntity payment);

    [Sqlx("UPDATE payments SET {{set:auto}} WHERE id = @id")]
    Task<int> UpdateStatusSuccessAsync(long id, string status, DateTime paidAt, DateTime updatedAt);

    [Sqlx("UPDATE payments SET {{set:auto}} WHERE id = @id")]
    Task<int> UpdateStatusFailedAsync(long id, string status, string errorMessage, DateTime updatedAt);
}

[RepositoryFor(typeof(IPaymentRepository))]
public partial class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnection connection;

    public PaymentRepository(IDbConnectionFactory connectionFactory)
    {
        connection = connectionFactory.CreateConnection();
    }
}
