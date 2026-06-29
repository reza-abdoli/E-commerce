using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.ViewComponents
{
    public class SiteHeaderViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public SiteHeaderViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            ViewData["Categories"] = await _categoryService.GetAllActiveCategories();
            return View("SiteHeader");
        }
    }
}
