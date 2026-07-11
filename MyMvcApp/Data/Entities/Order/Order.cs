using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities.Order
{
    public class Order : BaseEntity
    {
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? DestinationCity { get; set; }
        public string? Address { get; set; }
        public string? PostCode { get; set; }
        public int TotalPrice { get; set; }
        public string? Description { get; set; }
        public string? PostTraceCode { get; set; }
        public long? TransactionId { get; set; }
        public long? PaymentRecordId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public OrderState OrderState { get; set; }
        public User.User User { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public enum OrderState
    {
        [Display(Name = "ثبت شده")]
        Open,
        [Display(Name = "پرداخت شده")]
        Paid,
        [Display(Name = "در حال پردازش")]
        Pending,
        [Display(Name = "ارسال شده")]
        Sent,
        [Display(Name = "موجوع شده")]
        Returned,
        [Display(Name = "لغو شده")]
        Canceled,
    }
}
