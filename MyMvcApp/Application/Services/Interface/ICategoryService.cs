using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs;
using Data.DTOs.Category;
using Data.Entities;

namespace Application.Services.Interface;

public interface ICategoryService : IAsyncDisposable
{
    Task<bool> CreateCategory(CreateCategoryDto dto);
    Task<FilterCategoryDto> FilterCategory(FilterCategoryDto dto);
    Task<List<ProductCategory>> GetAllActiveCategories();
    Task<List<ProductCategory>> GetAllChildsCategory(long? thisId);
    Task<bool> AddProductSelectedCategories(List<long> selectedCategories, long productId);
    Task RemoveProductSelectedCategories(long productId);
}