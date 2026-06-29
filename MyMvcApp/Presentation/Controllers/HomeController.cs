using System.Diagnostics;
using Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers;

public class HomeController : SiteBaseController
{
    private readonly ICommonService _commonService;
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;

    public HomeController(ICommonService commonService, ICategoryService categoryService, IProductService productService)
    {
        _commonService = commonService;
        _categoryService = categoryService;
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Banners"] = await _commonService.GetHomeBanners();
        ViewData["Categories"] =  await _categoryService.GetAllActiveCategories();
        ViewData["Products"] = await _productService.GetNewestProducts();
        return View();
    }

}
