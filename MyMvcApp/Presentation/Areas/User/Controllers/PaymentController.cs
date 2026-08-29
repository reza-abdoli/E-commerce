using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
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


            if (openOrder.TotalCartPrice() > 100000000)
            {
                TempData[ErrorMessage] = "Orders exceeding 100 million Tomans cannot be paid.";
                return RedirectToAction("Cart", "Order");
            }

            // Wallet Payment Logic
            if (isWalletPayment)
            {
                if (openOrder.TotalCartPrice() > await _userService.GetUserBalance(userId))
                {
                    TempData[ErrorMessage] = "Your wallet balance is insufficient.";
                    return RedirectToAction("Index", "Home");
                }

                var isSuccess = await _paymentFacadeService.PayOrderWithWalletAsync(
                    userId,
                    openOrder.TotalCartPrice(),
                    openOrder.Order.Id);

                if (isSuccess)
                {
                    TempData[SuccessMessage] = "Your payment was successful.";
                    return RedirectToAction("PaidOrders", "Order", new { area = "User" });
                }

                TempData[ErrorMessage] = "An error occurred while processing the payment or inventory.";
                return RedirectToAction("Cart", "Order");
            }

            // Online Payment Pre-Process: Create Pending Transaction before redirecting to bank
            var authority = Guid.NewGuid().ToString("N"); // Simulated Bank Gateway Token
            await _paymentService.CreatePendingTransactionAsync(userId, openOrder.Order.Id, openOrder.TotalCartPrice(), authority);

            // Redirect user to payment gateway page (Simulated view or bank endpoint)
            ViewBag.Authority = authority;
            return View("GoToGateway", openOrder);
        }

        [HttpPost("pay-order-callback")]
        public async Task<IActionResult> PayOrderCallback(string authority, bool isSuccess)
        {
            var pendingTransaction = await _paymentService.GetTransactionByAuthorityAsync(authority);

            if (pendingTransaction == null)
            {
                TempData[ErrorMessage] = "Invalid payment transaction context.";
                return RedirectToAction("Cart", "Order", new { area = "User" });
            }

            if (!isSuccess)
            {
                await _paymentService.MarkTransactionAsFailedAsync(pendingTransaction.Id);
                TempData[ErrorMessage] = "Your payment was unsuccessful.";
                return RedirectToAction("Cart", "Order", new { area = "User" });
            }

            // Finalize order via Facade transaction
            var isFinalized = await _paymentFacadeService.FinalizeOnlineOrderPaymentAsync(pendingTransaction);

            if (isFinalized)
            {
                TempData[SuccessMessage] = "Your payment was successful.";
                return RedirectToAction("PaidOrders", "Order", new { area = "User" });
            }

            TempData[ErrorMessage] = "An error occurred while finalizing order. Refunded to wallet if deducted.";
            return RedirectToAction("Cart", "Order", new { area = "User" });
        }

        [HttpPost("add-balance")]
        public async Task<IActionResult> AddBalance(DepositBalanceDto dto)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            if (dto.Amount <= 0)
            {
                TempData[ErrorMessage] = "Amount must be greater than zero!";
                return RedirectToAction("Index", "Home");
            }

            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isProfileComplete = await _userService.IsProfileDetailCompleted(userId);

            if (!isProfileComplete)
                return RedirectToAction("EditUserDetail", "Account", new { area = "User" });

            // Create Pending Transaction before bank redirect for wallet charging
            var authority = Guid.NewGuid().ToString("N");
            await _paymentService.CreatePendingTransactionAsync(userId, null, dto.Amount, authority);

            ViewBag.Authority = authority;
            return View("PayBalance", dto);
        }

        [HttpPost("pay-balance-callback")]
        public async Task<IActionResult> PayBalanceCallback(string authority, bool isSuccess)
        {
            var pendingTransaction = await _paymentService.GetTransactionByAuthorityAsync(authority);

            if (pendingTransaction == null)
            {
                TempData[ErrorMessage] = "Invalid payment transaction context.";
                return RedirectToAction("Index", "Home", new { area = "User" });
            }

            if (isSuccess)
            {
                var isDeposited = await _paymentFacadeService.DepositWalletBalanceAsync(pendingTransaction);

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
                await _paymentService.MarkTransactionAsFailedAsync(pendingTransaction.Id);
                TempData[ErrorMessage] = "Your payment was unsuccessful.";
            }

            return RedirectToAction("Index", "Home", new { area = "User" });
        }
    }
}