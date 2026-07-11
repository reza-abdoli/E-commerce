using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Order;
using Data.Entities.Order;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.Controllers
{
    public class PaymentController : UserBaseController
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;

        public PaymentController(IUserService userService, IOrderService orderService)
        {
            _userService = userService;
            _orderService = orderService;
        }


        [HttpGet("pay-order")]
        public async Task<IActionResult> PayOrder()
        {
            var openOrder = await _orderService.UserOpenOrderDetail(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            if (openOrder == null)
            {
                TempData[ErrorMessage] = "سبد خرید شما خالی است";
                return RedirectToAction("Cart", "Order");
            }

            if (openOrder.TotalCartPrice() * 10 > 1000000000)
            {
                TempData[ErrorMessage] = "سفارشی که بیش از 100 میلیون تومان باشد قابل پرداخت نیست . لطغا از تعداد سفارش خود بکاهید.";
                return RedirectToAction("Cart", "Order");
            }

            return View(openOrder);
        }

        [HttpPost("pay-order")]
        public async Task<IActionResult> PayOrder(bool isSuccess)
        {
            if (isSuccess)
            {
                var openOrder = await _orderService.UserOpenOrderDetail(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));

                SuccessPaymentResult result = new SuccessPaymentResult
                {
                    trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                    amount = openOrder.TotalCartPrice() * 10,
                    order_id = openOrder.Order.Id,
                    buyer_id = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                    payment_time = DateTime.UtcNow
                };
                await _orderService.UpdatePaidOrder(result);
                TempData[SuccessMessage] = "پرداخت شما با موفقیت انجام شد.";
                return RedirectToAction("PaidOrders", "Order", new { area = "User"});
            }
            else
            {
                TempData[ErrorMessage] = "پرداخت شما ناموفق بود.";
                return RedirectToAction("Cart", "Order", new { area = "User" });
            }

            
        }
    }
}