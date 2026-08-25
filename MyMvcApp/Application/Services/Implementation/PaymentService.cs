using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.Entities.Order;
using Data.Repository;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation
{

    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentRecord> _paymentRecordRepository;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IRepository<PaymentRecord> paymentRecordRepository, ILogger<PaymentService> logger)
        {
            _paymentRecordRepository = paymentRecordRepository;
            _logger = logger;
        }
        public async ValueTask DisposeAsync()
        {
            await _paymentRecordRepository.DisposeAsync();
        }


        public async Task<long> SavePaymentRecord(SuccessPaymentResult result)
        {
            var record = new PaymentRecord
            {
                Amount = result.amount,
                TransactionId = result.trans_id,
                PaymentTime = result.payment_time,
                BuyerId = result.buyer_id,
                OrderId = result.order_id
            };
            await _paymentRecordRepository.AddEntity(record);
            await _paymentRecordRepository.SaveChanges();

            if (result.order_id == null)
                _logger.LogInformation(
                    "Wallet charged. PaymentRecordId: {PaymentRecordId}, BuyerId: {BuyerId}, Amount: {Amount}, TransactionId: {TransactionId}",
                    record.Id, record.BuyerId, record.Amount, record.TransactionId);
            else
            {
                _logger.LogInformation(
                    "Order paid. PaymentRecordId: {PaymentRecordId}, OrderId: {OrderId}, BuyerId: {BuyerId}, Amount: {Amount}, TransactionId: {TransactionId}",
                    record.Id, record.OrderId, record.BuyerId, record.Amount, record.TransactionId);
            }

            return record.Id;
        }
    }
}