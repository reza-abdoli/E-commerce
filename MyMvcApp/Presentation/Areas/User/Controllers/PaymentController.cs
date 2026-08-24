using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Data.DTOs.Order;
using Data.Entities.Order;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.Controllers
{
    public class PaymentController : UserBaseController
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public PaymentController(IUserService userService, IOrderService orderService, IPaymentService paymentService)
        {
            _userService = userService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpGet("pay-order")]
        public async Task<IActionResult> PayOrder(bool isWalletPayment)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var openOrder = await _orderService.UserOpenOrderDetail(userId);

            if (openOrder == null)
            {
                TempData[ErrorMessage] = "سبد خرید شما خالی است";
                return RedirectToAction("Cart", "Order");
            }

            if (openOrder.TotalCartPrice() * 10 > 1000000000)
            {
                TempData[ErrorMessage] = "سفارشی که بیش از 100 میلیون تومان باشد قابل پرداخت نیست.";
                return RedirectToAction("Cart", "Order");
            }

            if (isWalletPayment)
            {
                if (openOrder.TotalCartPrice() > await _userService.GetUserBalance(userId))
                {
                    TempData[ErrorMessage] = "موجودی کیف پول شما کافی نیست.";
                    return RedirectToAction("Index", "Home");
                }
                var result = new SuccessPaymentResult
                {
                    trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                    amount = openOrder.TotalCartPrice() * 10,
                    order_id = openOrder.Order.Id,
                    buyer_id = userId,
                    payment_time = DateTime.UtcNow
                };
                var recordId = await _paymentService.SavePaymentRecord(result);
                await _orderService.ProcessSuccessfulOrder(result, recordId);
                await _userService.WithdrawBalance(userId, openOrder.TotalCartPrice());
                TempData[SuccessMessage] = "پرداخت شما با موفقیت انجام شد.";
                return RedirectToAction("PaidOrders", "Order", new { area = "User" });

            }

            return View(openOrder);
        }

        [HttpPost("pay-order")]
        public async Task<IActionResult> PayOrder(bool isSuccess, bool isWalletPayment)
        {
            if (!isSuccess)
            {
                TempData[ErrorMessage] = "پرداخت شما ناموفق بود.";
                return RedirectToAction("Cart", "Order", new { area = "User" });
            }
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var openOrder = await _orderService.UserOpenOrderDetail(userId);
            var result = new SuccessPaymentResult
            {
                trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                amount = openOrder.TotalCartPrice() * 10,
                order_id = openOrder.Order.Id,
                buyer_id = userId,
                payment_time = DateTime.UtcNow
            };
            var recordId = await _paymentService.SavePaymentRecord(result);
            await _orderService.ProcessSuccessfulOrder(result, recordId);
            TempData[SuccessMessage] = "پرداخت شما با موفقیت انجام شد.";
            return RedirectToAction("PaidOrders", "Order", new { area = "User" });
        }

        [HttpPost("add-balance")]
        public async Task<IActionResult> AddBalance(DepositBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            if (dto.Amount < 0)
            {
                TempData[ErrorMessage] = "Could not be negetive!";
                return RedirectToAction("Index", "Home");
            }
            var result = await _userService.IsProfileDetailCompleted(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            if (!result)
                return RedirectToAction("EditUserDetail", "Account", new { area = "User" });

            return View("PayBalance", dto);
        }


        [HttpPost("pay-balance")]
        public async Task<IActionResult> PayBalance(bool isSuccess, DepositBalanceDto dto)
        {
            if (isSuccess)
            {
                SuccessPaymentResult result = new SuccessPaymentResult
                {
                    trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                    amount = dto.Amount * 10,
                    buyer_id = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value),
                    payment_time = DateTime.UtcNow
                };
                await _paymentService.SavePaymentRecord(result);
                await _userService.DepositBalance(dto);
                TempData[SuccessMessage] = "پرداخت شما با موفقیت انجام شد.";
            }
            else
            {
                TempData[ErrorMessage] = "پرداخت شما ناموفق بود.";
            }
            return RedirectToAction("Index", "Home", new { area = "User" });
        }
    }
}