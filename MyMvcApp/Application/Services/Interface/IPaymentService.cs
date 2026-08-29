using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities.Order;

namespace Application.Services.Interface
{
    public interface IPaymentService : IAsyncDisposable
    {
        Task<PaymentTransaction> CreatePendingTransactionAsync(long userId, long? orderId, int amount, string authority);
        Task<PaymentTransaction?> GetTransactionByAuthorityAsync(string authority);
        Task UpdateTransactionStatusAsync(long transactionId, PaymentStatus status);
        Task MarkTransactionAsFailedAsync(long transactionId);
    }
}