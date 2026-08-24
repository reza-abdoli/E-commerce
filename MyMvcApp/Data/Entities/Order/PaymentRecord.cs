using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.Order
{
    public class PaymentRecord : BaseEntity
    {
        public long TransactionId { get; set; }

        public int Amount { get; set; }
        public long BuyerId { get; set; }
        public long? OrderId { get; set; }

        public DateTime PaymentTime { get; set; }
    }
}