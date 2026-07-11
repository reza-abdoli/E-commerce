using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Data.DTOs.Order
{
    public class FilterOrderDto 
    {
        public long? UserId { get; set; }
        public int? MinimumPrice { get; set; }
        public string UserName { get; set; }
        public string DestinationCity { get; set; }
        // public string Description { get; set; }
        public long? TransactionId { get; set; }
        public long? PaymentRecordId { get; set; }
        public FilterOrderState FilterOrderState { get; set; }
        public List<Data.Entities.Order.Order> Data { get; set; }

    }
    public enum FilterOrderState
    {
        [Display(Name = "همه")]
        All,
        [Display(Name = "ثبت شده")]
        Open,
        [Display(Name = "پرداخت شده")]
        Paid,
        [Display(Name = "ارسال شده")]
        Sent,
        [Display(Name = "لغو شده")]
        Canceled,
        [Display(Name = "موجوع شده")]
        Returned,
        [Display(Name = "در حال پردازش")]
        Pending
    }
}