using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Order;
using Data.Entities.Order;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Areas.Admin.Controllers;

namespace Presentation.Areas.Admin.Controllers;

public class OrderController : AdminBaseController
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    public OrderController(IProductService productService, IOrderService orderService)
    {
        _productService = productService;
        _orderService = orderService;
    }

        [HttpGet("admin-filter-orders")]
        public async Task<IActionResult> FilterOrder(FilterOrderDto filter)
        {
            var model = await _orderService.FilterOrders(filter);
            return View(model);
        }

        [HttpGet("admin-order-detail-{orderId}")]
        public async Task<IActionResult> OrderDetail(long orderId)
        {
            // get all states for admin to change state of an order base on this options
            ViewData["States"] = Enum.GetValues<OrderState>();
            var model = await _orderService.OrderDetail(orderId);
            return View(model);
        }
        [HttpPost("process-order") , ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(long orderId, OrderState orderState)
        {
            if (!ModelState.IsValid) 
                return RedirectToAction("OrderDetail" , new {area = "Admin" , orderId = orderId});
            await _orderService.ProcessOrder(orderId, orderState);
            TempData[SuccessMessage] = SuccessText;
            return RedirectToAction("OrderDetail" , new {area = "Admin" , orderId = orderId});
        }
}