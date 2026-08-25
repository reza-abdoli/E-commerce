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
using Microsoft.Extensions.Logging;
using static Data.DTOs.Category.FilterCategoryDto;

namespace Application.Services.Implementation;

public class CategoryService : ICategoryService
{
    private readonly IRepository<ProductCategory> _categoryRepository;
    private readonly IRepository<ProductSelectedCategory> _productSelectedCategory;
    private readonly ILogger<CategoryService> _logger;


    public CategoryService(IRepository<ProductCategory> categoryRepository, IRepository<ProductSelectedCategory> productSelectedCategory, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _productSelectedCategory = productSelectedCategory;
        _logger = logger;
    }
    public async ValueTask DisposeAsync()
    {
        await _categoryRepository.DisposeAsync();
    }
    public async Task<bool> CreateCategory(CreateCategoryDto dto)
    {

        var urlInUse = await _categoryRepository.GetQuery().AnyAsync(c => c.Url == dto.Url);
        if (urlInUse)
        {
            _logger.LogWarning("Category creation failed: URL already in use. Url: {Url}", dto.Url);
            return false;
        }

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
            _logger.LogWarning("Category creation failed: image save failed. Title: {Title}", dto.Title);
            return false;
        }
        await _categoryRepository.AddEntity(category);
        await _categoryRepository.SaveChanges();

        _logger.LogInformation("Category created. CategoryId: {CategoryId}, Title: {Title}, ParentId: {ParentId}", category.Id, category.Title, category.ParentId);
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
        foreach (var item in selectedCategories) 
        {
            if (await _categoryRepository.GetEntityById(item) == null)
            {
                _logger.LogWarning("Adding product categories failed: category not found. ProductId: {ProductId}, CategoryId: {CategoryId}", productId, item);
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

        _logger.LogInformation("Product categories assigned. ProductId: {ProductId}, CategoryIds: {CategoryIds}", productId, string.Join(",", selectedCategories));
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

        _logger.LogInformation("Product categories removed. ProductId: {ProductId}, Count: {Count}", productId, selectedCategories.Count);
    }
}