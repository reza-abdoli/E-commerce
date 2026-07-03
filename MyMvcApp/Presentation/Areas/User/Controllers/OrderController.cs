using System.Security.Claims;
using Application.Services.Interface;
using Data.DTOs.Order;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.Controllers
{
    public class OrderController : UserBaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ─── وضعیت ورینت در سبد خرید (برای لود اولیه صفحه) ───
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

        // ─── افزودن به سبد خرید — AJAX ───
        [HttpPost("add-product-to-cart")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToCart(SubmitOrderDetailDto dto)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            dto.UserId = userId;

            var orderDetailId = await _orderService.AddProductToOrder(dto);

            return Json(new { success = true, orderDetailId });
        }

        // ─── تغییر تعداد یا حذف از سبد — AJAX ───
        [HttpPost("change-order-detail-count")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOrderDetailCount(long orderDetailId, int newCount)
        {
            var res = await _orderService.ChangeOrderDetailCount(orderDetailId, newCount);
            if(res)
                return Json(new { success = true });
            return Json(new { success = false });
            
        }
    }
}