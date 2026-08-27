using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Data.Entities.Order;
using Microsoft.AspNetCore.Mvc;


namespace Presentation.Areas.User.Controllers
{
    public class PaymentController : UserBaseController
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentFacadeService _paymentFacadeService;


        public PaymentController(
            IUserService userService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPaymentFacadeService paymentFacadeService)
        {
            _userService = userService;
            _orderService = orderService;
            _paymentService = paymentService;
            _paymentFacadeService = paymentFacadeService;
        }

        [HttpGet("pay-order")]
        public async Task<IActionResult> PayOrder(bool isWalletPayment)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var openOrder = await _orderService.UserOpenOrderDetail(userId);

            if (openOrder == null)
            {
                TempData[ErrorMessage] = "Your shopping cart is empty.";
                return RedirectToAction("Cart", "Order");
            }


            // Maximum allowable order amount check (100M Tomans)
            if (openOrder.TotalCartPrice() > 100000000)
            {
                TempData[ErrorMessage] = "Orders exceeding 100 million Tomans cannot be paid.";
                return RedirectToAction("Cart", "Order");
            }

            if (isWalletPayment)
            {
                if (openOrder.TotalCartPrice() > await _userService.GetUserBalance(userId))
                {
                    TempData[ErrorMessage] = "Your wallet balance is insufficient.";
                    return RedirectToAction("Index", "Home");
                }
                var result = new SuccessPaymentResult
                {
                    trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                    amount = openOrder.TotalCartPrice(),
                    order_id = openOrder.Order.Id,
                    buyer_id = userId,
                    payment_time = DateTime.UtcNow
                };


                // Execute atomic wallet payment process via Facade
                var isSuccess = await _paymentFacadeService.PayOrderWithWalletAsync(
                    userId,
                    openOrder.TotalCartPrice(),
                    result);


                if (isSuccess)
                {
                    TempData[SuccessMessage] = "Your payment was successful.";
                    return RedirectToAction("PaidOrders", "Order", new { area = "User" });
                }


                TempData[ErrorMessage] = "An error occurred while processing the payment or inventory.";
                return RedirectToAction("Cart", "Order");
            }

            return View(openOrder);
        }

        [HttpPost("pay-order")]
        public async Task<IActionResult> PayOrder(bool isSuccess, bool isWalletPayment)
        {
            if (!isSuccess)
            {
                TempData[ErrorMessage] = "Your payment was unsuccessful.";
                return RedirectToAction("Cart", "Order", new { area = "User" });
            }
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var openOrder = await _orderService.UserOpenOrderDetail(userId);

            if (openOrder == null)
            {
                TempData[ErrorMessage] = "No order was found.";
                return RedirectToAction("Cart", "Order", new { area = "User" });
            }

            var result = new SuccessPaymentResult
            {
                trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                amount = openOrder.TotalCartPrice(),
                order_id = openOrder.Order.Id,
                buyer_id = userId,
                payment_time = DateTime.UtcNow
            };


            // Finalize online order payment via Facade transaction
            var isFinalized = await _paymentFacadeService.FinalizeOnlineOrderPaymentAsync(result);


            if (isFinalized)
            {
                TempData[SuccessMessage] = "Your payment was successful.";
                return RedirectToAction("PaidOrders", "Order", new { area = "User" });
            }


            TempData[ErrorMessage] = "An error occurred while finalizing the order or deducting inventory.";
            return RedirectToAction("Cart", "Order", new { area = "User" });
        }

        [HttpPost("add-balance")]
        public async Task<IActionResult> AddBalance(DepositBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");


            if (dto.Amount < 0)
            {
                TempData[ErrorMessage] = "Amount cannot be negative!";
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
                var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var result = new SuccessPaymentResult
                {
                    trans_id = Random.Shared.NextInt64(1000000000, 9999999999),
                    amount = dto.Amount * 10,
                    buyer_id = userId,
                    payment_time = DateTime.UtcNow
                };

                var isDeposited = await _paymentFacadeService.DepositWalletBalanceAsync(dto, result);

                if (isDeposited)
                {
                    TempData[SuccessMessage] = "Account balance was successfully charged.";
                }
                else
                {
                    TempData[ErrorMessage] = "An error occurred while charging your account balance.";
                }
            }
            else
            {
                TempData[ErrorMessage] = "Your payment was unsuccessful.";
            }

            return RedirectToAction("Index", "Home", new { area = "User" });
        }
    }
}