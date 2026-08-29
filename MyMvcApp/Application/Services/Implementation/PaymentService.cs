using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.Entities.Order;
using Data.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Implementation
{

    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentTransaction> _paymentTransactionRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IRepository<PaymentTransaction> paymentTransactionRepository,
            ILogger<PaymentService> logger)
        {
            _paymentTransactionRepository = paymentTransactionRepository;
            _logger = logger;
        }

        public async Task<PaymentTransaction> CreatePendingTransactionAsync(long userId, long? orderId, int amount, string authority)
        {
            var transaction = new PaymentTransaction
            {
                UserId = userId,
                OrderId = orderId,
                Amount = amount,
                Authority = authority,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _paymentTransactionRepository.AddEntity(transaction);
            await _paymentTransactionRepository.SaveChanges();

            _logger.LogInformation(
                "Pending transaction created. Id: {Id}, UserId: {UserId}, OrderId: {OrderId}, Amount: {Amount}, Authority: {Authority}",
                transaction.Id, userId, orderId, amount, authority);

            return transaction;
        }

        public async Task<PaymentTransaction?> GetTransactionByAuthorityAsync(string authority)
        {
            return await _paymentTransactionRepository.GetQuery().SingleOrDefaultAsync(t => t.Authority == authority);
        }

        public async Task UpdateTransactionStatusAsync(long transactionId, PaymentStatus status)
        {
            var transaction = await _paymentTransactionRepository.GetEntityById(transactionId);
            if (transaction != null)
            {
                transaction.Status = status;
                transaction.UpdatedAt = DateTime.UtcNow;

                _paymentTransactionRepository.UpdateEntity(transaction);
                await _paymentTransactionRepository.SaveChanges();

                _logger.LogInformation(
                    "Transaction status updated. Id: {Id}, NewStatus: {Status}",
                    transaction.Id, status);
            }
        }

        public async Task MarkTransactionAsFailedAsync(long transactionId)
        {
            await UpdateTransactionStatusAsync(transactionId, PaymentStatus.Failed);
        }

        public async ValueTask DisposeAsync()
        {
            await _paymentTransactionRepository.DisposeAsync();
        }
    }

}