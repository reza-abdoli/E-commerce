using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs;
using Data.DTOs.Category;
using Data.DTOs.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyMvcApp.Areas.Admin.Controllers;
using Application.Extensions;

namespace Presentation.Areas.Admin.Controllers;

public class ProductController : AdminBaseController
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    public ProductController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }
    #region Color

    [HttpGet("create-color")]
    public IActionResult CreateColor()
    {
        return View();
    }
    [HttpPost("create-color"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateColor(CreateColorDTo dto)
    {
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "عنوان یا کد رنگ نباید خالی باشد";
            return View(dto);
        }
        await _productService.CreateColor(dto);

        TempData[SuccessMessage] = SuccessText;
        return RedirectToAction("FilterColor");
    }

    [HttpGet("filter-color")]
    public async Task<IActionResult> FilterColor(FilterColorDto dto)
    {
        var filter = await _productService.FilterColors(dto);
        return View(filter);
    }

    #endregion

    #region Brand
    [HttpGet("create-brand")]
    public IActionResult CreateBrand()
    {
        return View();
    }
    [HttpPost("create-brand"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBrand(CreateBrandDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "تایتل یا Url نباید خالی باشد";
            return View(dto);
        }
        if (!await _productService.CreateBrand(dto))
        {
            TempData[ErrorMessage] = "ذخیره عکس ناموفق بود";
            return View(dto);
        }

        TempData[SuccessMessage] = SuccessText;
        return RedirectToAction("FilterBrand");
    }

    [HttpGet("filter-brand")]
    public async Task<IActionResult> FilterBrand(FilterBrandDto dto)
    {
        var filter = await _productService.FilterBrands(dto);
        return View(filter);
    }
    #endregion


    #region Category
    [HttpGet("create-category")]
    public async Task<IActionResult> CreateCategory()
    {
        ViewData["ParentCategory"] = await _categoryService.GetAllActiveCategories();
        return View();
    }

    [HttpPost("create-category"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
    {
        ViewData["ParentCategory"] = await _categoryService.GetAllActiveCategories();
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "مشخصات را کامل وارد کنید";
            return View(dto);
        }
        if (!await _categoryService.CreateCategory(dto))
        {
            TempData[ErrorMessage] = "url تکراری می باشد";
            return View(dto);
        }

        TempData[SuccessMessage] = SuccessText;
        return RedirectToAction("FilterCategory");
    }

    [HttpGet("filter-category")]
    public async Task<IActionResult> FilterCategory(FilterCategoryDto dto)
    {
        var filter = await _categoryService.FilterCategory(dto);
        ViewData["ParentCategory"] = await _categoryService.GetAllActiveCategories();
        return View(filter);
    }
    #endregion

    #region Size
    [HttpGet("create-size")]
    public IActionResult CreateSize()
    {
        return View();
    }
    [HttpPost("create-size"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSize(CreateSizeDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "مشخصات را کامل وارد کنید";
            return View(dto);
        }
        await _productService.CreateSize(dto);
      
        TempData[SuccessMessage] = SuccessText;
        return RedirectToAction("FilterSize");
    }

    [HttpGet("filter-size")]
    public async Task<IActionResult> FilterSize(FilterSizeDto dto)
    {
        var filter = await _productService.FilterSizes(dto);
        return View(filter);
    }
    #endregion

    #region Product
    [HttpGet("create-product")]
    public async Task<IActionResult> CreateProduct()
    {
        ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
        ViewData["Brands"] = await _productService.GetAllBrands();
        return View(new CreateProductDto());
    }

    [HttpPost("create-product"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
        ViewData["Brands"] = await _productService.GetAllBrands();
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "مشخصات را کامل وارد کنید";
            return View(dto);
        }
        if (!dto.MainImage.IsImage())
        {
            TempData[ErrorMessage] = "فرمت تصویر اصلی محصول قابل قبول نیست";
            return View(dto);
        }

        if (dto.ProductGalleries != null)
        {
            if (dto.ProductGalleries.Any(image => !image.IsImage()))
            {
                TempData[ErrorMessage] = "فرمت تصویر انتخاب شده برای گالری محصول قابل قبول نیست";
                return View(dto);
            }
        }
        var result = await _productService.CreateProduct(dto);
        switch (result)
        {

            case CreateProductResult.Error:
                TempData[ErrorMessage] = "عملیات با خطا مواجه شد";
                break;
            case CreateProductResult.SavingMainImageFailed:
                TempData[ErrorMessage] = "ذخیره ی تصویر محصول با خطا مواجه شد";
                break;
            case CreateProductResult.BrandNotFound:
                TempData[ErrorMessage] = "برند یافت نشد";
                break;
            case CreateProductResult.CategoryNotFound:
                TempData[ErrorMessage] = "دسته بندی انتخاب شده یافت نشد";
                break;
            case CreateProductResult.Success:
                TempData[SuccessMessage] = "محصول با موفقیت ایجاد شد.";
                return RedirectToAction("FilterProduct");
            default:
                throw new ArgumentOutOfRangeException();
        }

        return RedirectToAction("FilterProduct");
    }

    [HttpGet("filter-product")]

    public async Task<IActionResult> FilterProduct(FilterProductDto dto)
    {
        var filter = await _productService.FilterProduct(dto);
        ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
        ViewData["Brands"] = await _productService.GetAllBrands();
        return View(filter);
    }

    [HttpGet("edit-product")]

    public async Task<IActionResult> EditProduct(long productId)
    {
        var previousProduct = await _productService.GetEditProduct(productId);
        ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
        ViewData["Brands"] = await _productService.GetAllBrands();
        return View(previousProduct);
    }

    [HttpPost("edit-product")]

    public async Task<IActionResult> EditProduct(EditProductDto dto)
    {
        ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
        ViewData["Brands"] = await _productService.GetAllBrands();

        if (dto.MainImage != null && !dto.MainImage.IsImage())
        {
            TempData[ErrorMessage] = "فرمت تصویر اصلی محصول قابل قبول نیست";
            return View(dto);
        }

        if (!ModelState.IsValid) return View(dto);

        var result = await _productService.EditProduct(dto);

        switch (result)
        {

            case EditProductResult.Error:
                TempData[ErrorMessage] = "عملیات با خطا مواجه شد";
                break;
            case EditProductResult.ImageNotSaved:
                TempData[ErrorMessage] = "در ذخیره سازی تصویر خطایی رخ داده است";
                break;
            case EditProductResult.BrandNotFound:
                TempData[ErrorMessage] = "برند یافت نشد";
                break;
            case EditProductResult.CategoryNotFound:
                TempData[ErrorMessage] = "دسته بندی انتخاب شده یافت نشد";
                break;
            case EditProductResult.Success:
                TempData[SuccessMessage] = "عملیات با موفقیت انجام شد.";
                return RedirectToAction("ProductDetail", new { area = "Admin", productId = dto.ProductId });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return View(dto);
    }

    [HttpGet("product-detail-{productId}")]

    public async Task<IActionResult> ProductDetail(long productId)
    {
        var productDetail = await _productService.GetProductDetail(productId);
        ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
        ViewData["Brands"] = await _productService.GetAllBrands();
        return View(productDetail);
    }


    #endregion
}