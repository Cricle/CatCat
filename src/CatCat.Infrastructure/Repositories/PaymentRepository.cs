using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;
using PaymentEntity = CatCat.Infrastructure.Entities.Payment;

namespace CatCat.Infrastructure.Repositories;

public interface IPaymentRepository
{
    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE id = @id")]
    Task<PaymentEntity?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE order_id = @orderId")]
    Task<PaymentEntity?> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT {{columns}} FROM {{table}} WHERE payment_intent_id = @paymentIntentId")]
    Task<PaymentEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);

    [Sqlx("INSERT INTO {{table}} ({{columns --exclude Id}}) VALUES ({{values}})")]
    Task<int> CreateAsync(PaymentEntity payment);

    [Sqlx("UPDATE {{table}} SET status = @status, paid_at = @paidAt, updated_at = @updatedAt WHERE id = @id")]
    Task<int> UpdateStatusSuccessAsync(long id, string status, DateTime paidAt, DateTime updatedAt);

    [Sqlx("UPDATE {{table}} SET status = @status, error_message = @errorMessage, updated_at = @updatedAt WHERE id = @id")]
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
