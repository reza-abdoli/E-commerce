using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities.Order;

namespace Data.Entities
{
    public class OrderDetail : BaseEntity
    {
        public long OrderId { get; set; }
        public long ProductVariantId { get; set; }
        public int ProductPrice { get; set; }
        public int TotalPrice { get; set; }
        public int Count { get; set; }
        public Order.Order Order { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
