using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Presentation.Areas.Admin.ViewComponents;

public class SelectedCategoriesViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;
    public SelectedCategoriesViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync(long? thisId, List<long>? selectedCategories, int level = 0)
    {
        var childsCategory = await _categoryService.GetAllChildsCategory(thisId);
        var model = new TreeViewCategoriesDto
        {
            Level = level,
            productCategories = childsCategory,
            SelectedCategoriesIds = selectedCategories
        };
        return View("SelectedCategories", model);
    }
    
}