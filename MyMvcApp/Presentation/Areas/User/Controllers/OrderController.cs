using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        [HttpPost("add-product-to-cart"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToCart(SubmitOrderDetailDto dto)
        {
            await _orderService.AddProductToOrder(dto);

            TempData[SuccessMessage] = "محصول به سبد خرید اضافه شد";
            return RedirectToAction("ProductDetail", "Product", new { area = "", productId = dto.ProductId });
        }

        [HttpPost("change-order-detail-count")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeOrderDetailCount(long orderDetailId, int newCount)
        {
            await _orderService.ChangeOrderDetailCount(orderDetailId, newCount);
            var model = await _orderService.UserOpenOrderDetail(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
            return PartialView("_CartContentPartial", model);
        }
    }
}