using CatCat.Infrastructure.Entities;
using CatCat.Infrastructure.Database;
using Sqlx.Annotations;
using System.Data;
using PaymentEntity = CatCat.Infrastructure.Entities.Payment;

namespace CatCat.Infrastructure.Repositories;

public interface IPaymentRepository
{
    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:id}}")]
    Task<PaymentEntity?> GetByIdAsync(long id);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:order_id}}")]
    Task<PaymentEntity?> GetByOrderIdAsync(long orderId);

    [Sqlx("SELECT {{columns:auto}} FROM {{table}} WHERE {{where:payment_intent_id}}")]
    Task<PaymentEntity?> GetByPaymentIntentIdAsync(string paymentIntentId);

    [Sqlx("{{insert:auto|exclude=Id}}")]
    Task<int> CreateAsync(PaymentEntity payment);

    [Sqlx("{{update}} SET {{set:status,paid_at,updated_at}} WHERE {{where:id}}")]
    Task<int> UpdateStatusSuccessAsync(long id, string status, DateTime paidAt, DateTime updatedAt);

    [Sqlx("{{update}} SET {{set:status,error_message,updated_at}} WHERE {{where:id}}")]
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
