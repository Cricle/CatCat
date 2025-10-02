using CatCat.Domain.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;
using PaymentEntity = CatCat.Domain.Entities.Payment;

namespace CatCat.Infrastructure.Repositories;

public interface IPaymentRepository
{
    [Sqlx("SELECT * FROM payments WHERE id = @id")]
    Task<PaymentEntity?> GetByIdAsync(long id);

    [Sqlx("SELECT * FROM payments WHERE order_id = @orderId")]
    Task<PaymentEntity?> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT * FROM payments WHERE payment_intent_id = @paymentIntentId")]
    Task<PaymentEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);

    [Sqlx("INSERT INTO payments (id, order_id, amount, currency, payment_intent_id, status, paid_at, error_message, created_at, updated_at) VALUES (@Id, @OrderId, @Amount, @Currency, @PaymentIntentId, @Status, @PaidAt, @ErrorMessage, @CreatedAt, @UpdatedAt)")]
    Task<int> CreateAsync(PaymentEntity payment);

    [Sqlx("UPDATE payments SET status = @status, paid_at = @paidAt, updated_at = @updatedAt WHERE id = @id")]
    Task<int> UpdateStatusSuccessAsync(long id, string status, DateTime paidAt, DateTime updatedAt);

    [Sqlx("UPDATE payments SET status = @status, error_message = @errorMessage, updated_at = @updatedAt WHERE id = @id")]
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
