using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.DTOs.Order
{
    public class SubmitOrderDetailDto
    {
        public long ProductId { get; set; }
        public long UserId { get; set; }
        public long ProductVariantId { get; set; }
        public int Count { get; set; }
    }
}
