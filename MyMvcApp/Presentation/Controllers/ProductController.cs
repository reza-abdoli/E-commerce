using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Product;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

    public class ProductController : SiteBaseController
    {
        #region CTOR
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }
        #endregion

        #region Filter Products
        [HttpGet("products-list")]
        public async Task<IActionResult> FilterProducts(FilterProductDto filter , string? url)
        {
            if (url != null) filter.categoryUrl = url;
            var model = await _productService.FilterProduct(filter);
            ViewData["Colors"] = await _productService.GetAllProductColors();
            ViewData["Sizes"] = await _productService.GetAllProductSizes();
            return View(model);
        }
        #endregion

        #region Product Detail
        [HttpGet("product-detail")]
        public async Task<IActionResult> ProductDetail(long productId)
        {
            var model = await _productService.GetProductDetail(productId);
            return View(model);
        }
        #endregion
    }