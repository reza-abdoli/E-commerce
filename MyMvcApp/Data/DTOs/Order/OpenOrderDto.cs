using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;
using Data.Entities.User;

namespace Data.DTOs.Order
{
    public class OpenOrderDto
    {
        public User User { get; set; }
        public Data.Entities.Order.Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        public int TotalCartPrice()
        {
            return OrderDetails.Select(item => item.ProductVariant.Product.Price * item.Count)
                .Aggregate(0, (current, price) => current + price);
        }
    }
}
