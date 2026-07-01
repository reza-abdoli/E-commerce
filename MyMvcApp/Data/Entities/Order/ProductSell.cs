using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class ProductSell : BaseEntity
    {
        public long ProductId { get; set; }
        public int SellCount { get; set; }
        public long OrderId { get; set; }
        public int ProductPrice { get; set; }
        public DateTime SellDate { get; set; }
    }
}
