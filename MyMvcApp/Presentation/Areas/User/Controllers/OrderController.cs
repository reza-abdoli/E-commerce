using System.Security.Claims;
using Application.Services.Interface;
using Data.DTOs.Order;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.Controllers
{
    public class OrderController : UserBaseController
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public OrderController(IOrderService orderService, IUserService userService)
        {
            _orderService = orderService;
            _userService = userService;
        }

        [HttpGet("cart-variant-info")]
        public async Task<IActionResult> CartVariantInfo(long productVariantId)
        {
            if (!User.Identity!.IsAuthenticated)
                return Json(new { count = 0 });

            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var orderDetail = await _orderService.GetCartItemForVariant(userId, productVariantId);

            if (orderDetail == null)
                return Json(new { count = 0 });

            return Json(new { orderDetailId = orderDetail.Id, count = orderDetail.Count });
        }

        [HttpPost("add-product-to-cart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToCart(SubmitOrderDetailDto dto)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            dto.UserId = userId;

            var orderDetailId = await _orderService.AddProductToOrder(dto);

            return Json(new { success = true, orderDetailId });
        }


        [HttpPost("change-order-detail-count")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOrderDetailCount(long orderDetailId, int newCount)
        {
            var res = await _orderService.ChangeOrderDetailCount(orderDetailId, newCount);
            if (res)
                return Json(new { success = true });
            return Json(new { success = false });

        }

        [HttpGet("cart")]
        public async Task<IActionResult> Cart()
        {
            var order = await _orderService.UserOpenOrderDetail(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            return View(order);
        }

        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var result = await _userService.IsProfileDetailCompleted(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            if (!result)
                return RedirectToAction("EditUserDetail", "Account", new { area = "User", returnToCheckout = true });


            var model = await _orderService.UserOpenOrderDetail(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            ViewData["User"] = await _userService.GetUserByUsername(User.FindFirst(ClaimTypes.Name)!.Value);

            return View(model);
        }

        [HttpGet("paid-orders")]
        public async Task<IActionResult> PaidOrders()
        {
            var orders = await _orderService.GetUserPaidOrders(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            return View(orders);
        }
    }
}