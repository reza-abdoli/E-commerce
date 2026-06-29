using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using MyMvcApp.Areas.Admin.Controllers;

namespace Presentation.Areas.Admin.Controllers;

public class CommonController : AdminBaseController
{
    private readonly ICommonService _commonService;
    public CommonController(ICommonService commonService)
    {
        _commonService = commonService;
    }
    [HttpGet("banners")]
    public async Task<IActionResult> BannersList()
    {
        var model = await _commonService.GetAllBanners();
        return View(model);
    }

    [HttpGet("create-banner")]
    public IActionResult CreateBanner()
    {
        return View();
    }

    [HttpPost("create-banner")]
    public async Task<IActionResult> CreateBanner(CreateBannerDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        await _commonService.CreateBanner(dto);
        TempData[SuccessMessage] = SuccessText;
        return RedirectToAction("BannersList");
    }
}