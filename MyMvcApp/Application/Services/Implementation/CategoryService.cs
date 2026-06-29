using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Services.Interface;
using Data.DTOs;
using Data.DTOs.Category;
using Data.Entities;
using Data.Repository;
using Microsoft.EntityFrameworkCore;
using static Data.DTOs.Category.FilterCategoryDto;

namespace Application.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly IRepository<ProductCategory> _categoryRepository;
    private readonly IRepository<ProductSelectedCategory> _productSelectedCategory;


    public CategoryService(IRepository<ProductCategory> categoryRepository, IRepository<ProductSelectedCategory> productSelectedCategory)
    {
        _categoryRepository = categoryRepository;
        _productSelectedCategory = productSelectedCategory;
    }
    public async ValueTask DisposeAsync()
    {
        await _categoryRepository.DisposeAsync();
    }
    public async Task<bool> CreateCategory(CreateCategoryDto dto)
    {

        var urlInUse = await _categoryRepository.GetQuery().AnyAsync(c => c.Url == dto.Url);
        if (urlInUse) return false;

        var category = new ProductCategory
        {
            Title = dto.Title,
            IsActive = true,
            Order = dto.Order,
            ParentId = dto.ParentId,
            Url = dto.Url,
            ShowInHome = dto.ShowInHome
        };

        var mainImageName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.MainImage.FileName);
        var res = dto.MainImage.AddImageToServer(mainImageName, PathExtension.CategoryServer, 150, 150, PathExtension.CategoryThumbServer);
        if (res)
        {
            category.MainImage = mainImageName;
        }
        else
        {
            return false;
        }
        await _categoryRepository.AddEntity(category);
        await _categoryRepository.SaveChanges();
        return true;
    }

    public async Task<FilterCategoryDto> FilterCategory(FilterCategoryDto filter)
    {
        var query = _categoryRepository.GetQuery()
    .OrderByDescending(d => d.CreatedAt).AsQueryable();

   
        switch (filter.CategoryStatus)
        {
            case FilterCategoryStatus.All:
                break;
            case FilterCategoryStatus.Active:
                query = query.Where(d => d.IsActive);
                break;
            case FilterCategoryStatus.DeActive:
                query = query.Where(d => !d.IsActive);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    
        if (!string.IsNullOrEmpty(filter.Title))
        {
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{filter.Title}%"));
        }

        if (filter.ParentId is > 0)
        {
            query = query.Where(c => c.ParentId == filter.ParentId);
        }
        filter.Data = await query.ToListAsync();
        return filter;
    }

    public async Task<List<ProductCategory>> GetAllActiveCategories()
    {
        return await _categoryRepository.GetQuery().Where(c=> c.IsActive).Include(c=> c.Parent).ToListAsync();
    }

    public async Task<List<ProductCategory>> GetAllChildsCategory(long? thisId)
    {
        return await _categoryRepository.GetQuery()
            .Where(c=> c.ParentId == thisId).OrderBy(o=> o.Order).ToListAsync();
    }
    public async Task<bool> AddProductSelectedCategories(List<long> selectedCategories, long productId)
    {
        // List<long> selectedExistCategory = new List<long>();
        foreach (var item in selectedCategories) 
        {
            if (await _categoryRepository.GetEntityById(item) == null)
            {
                return false;
            }
        }
        foreach (var item in selectedCategories)
        {
            await _productSelectedCategory.AddEntity(new ProductSelectedCategory
            {
                ProductId = productId,
                CategoryId = item
            });
        }
        await _productSelectedCategory.SaveChanges();
        return true;
    }

    public async Task RemoveProductSelectedCategories(long productId)
    {
        var selectedCategories = await _productSelectedCategory.GetQuery()
            .Where(c => c.ProductId == productId).ToListAsync();
        foreach (var item in selectedCategories)
        {
            _productSelectedCategory.DeleteEntityPermanently(item);
        }
    }
}