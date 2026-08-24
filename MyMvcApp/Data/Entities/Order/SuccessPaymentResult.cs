using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.Order
{
    public class SuccessPaymentResult : BaseEntity
    {

        public long trans_id { get; set; }

        public int amount { get; set; }

        public long? order_id { get; set; }

        public long buyer_id { get; set; }

        public DateTime payment_time { get; set; }
    }
}