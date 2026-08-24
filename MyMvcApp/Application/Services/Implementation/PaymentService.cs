using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.Entities.Order;
using Data.Repository;

namespace Application.Services.Implementation
{

    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentRecord> _paymentRecordRepository;
        public PaymentService(IRepository<PaymentRecord> paymentRecordRepository)
        {
            _paymentRecordRepository = paymentRecordRepository;
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
            return record.Id;
        }
    }
}