using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Extensions;
using Application.Services.Interface;
using Data.DTOs;
using Data.DTOs.Product;
using Data.Entities;
using Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services.Implementation;

public class ProductService : IProductService
{
    private readonly IRepository<Brand> _brandRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<ProductCategory> _productCategoryRepository;
    private readonly IRepository<ProductColor> _productColorRepository;
    private readonly IRepository<ProductFeature> _productFeatureRepository;
    private readonly IRepository<ProductGallery> _productGalleryRepository;
    private readonly IRepository<ProductSelectedCategory> _productSelectedCategoryRepository;
    private readonly IRepository<ProductSize> _productSizeRepository;
    private readonly IRepository<ProductVariant> _productVariantRepository;
    private readonly ICategoryService _categoryService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProductService> _logger;



    public ProductService(IRepository<Brand> brandRepository, IRepository<Product> productRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductColor> productColorRepository, IRepository<ProductFeature> productFeatureRepository, IRepository<ProductGallery> productGalleryRepository, IRepository<ProductSelectedCategory> productSelectedCategoryRepository, IRepository<ProductSize> productSizeRepository, IRepository<ProductVariant> productVariantRepository, ICategoryService categoryService, IMemoryCache cashe, ILogger<ProductService> logger)
    {
        _brandRepository = brandRepository;
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productColorRepository = productColorRepository;
        _productFeatureRepository = productFeatureRepository;
        _productGalleryRepository = productGalleryRepository;
        _productSelectedCategoryRepository = productSelectedCategoryRepository;
        _productSizeRepository = productSizeRepository;
        _productVariantRepository = productVariantRepository;
        _categoryService = categoryService;
        _cache = cashe;
        _logger = logger;
    }


    public async ValueTask DisposeAsync()
    {
        await _brandRepository.DisposeAsync();
        await _productRepository.DisposeAsync();
        await _productCategoryRepository.DisposeAsync();
        await _productColorRepository.DisposeAsync();
        await _productFeatureRepository.DisposeAsync();
        await _productGalleryRepository.DisposeAsync();
        await _productSelectedCategoryRepository.DisposeAsync();
        await _productSizeRepository.DisposeAsync();
        await _productVariantRepository.DisposeAsync();
    }


    #region Color

    public async Task CreateColor(CreateColorDTo color)
    {
        var newColor = new ProductColor
        {
            Title = color.Title,
            ColorCode = color.ColorCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _productColorRepository.AddEntity(newColor);
        await _productColorRepository.SaveChanges();

        _logger.LogInformation("Product color created. ColorId: {ColorId}, Title: {Title}", newColor.Id, newColor.Title);
    }

    public async Task<FilterColorDto> FilterColors(FilterColorDto filter)
    {
        var query = _productColorRepository.GetQuery().OrderByDescending(c => c.CreatedAt).AsQueryable();
        if (!string.IsNullOrEmpty(filter.Title))
        {
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{filter.Title}%"));
        }
        return new FilterColorDto
        {
            Title = filter.Title,
            Data = await query.ToListAsync()
        };
    }

    public async Task<List<ProductColor>> GetAllProductColors()
    {
        return await _productColorRepository.GetQuery().ToListAsync();
    }
    #endregion

    #region Brand

    public async Task<FilterBrandDto> FilterBrands(FilterBrandDto filter)
    {
        var query = _brandRepository.GetQuery().OrderByDescending(c => c.CreatedAt).AsQueryable();
        if (!string.IsNullOrEmpty(filter.Title))
        {
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{filter.Title}%"));
        }
        if (!string.IsNullOrEmpty(filter.Url))
        {
            query = query.Where(c => EF.Functions.Like(c.Url, $"%{filter.Url}%"));
        }
        return new FilterBrandDto
        {
            Title = filter.Title,
            Url = filter.Url,
            Data = await query.ToListAsync()
        };

    }

    public async Task<bool> CreateBrand(CreateBrandDto dto)
    {
        var newBrand = new Brand
        {
            Title = dto.Title,
            Url = dto.Url,
            Order = dto.Order,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var uniqueImgName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.Image.FileName);
        var uploadingImgResult = dto.Image.AddImageToServer(uniqueImgName, PathExtension.BrandServer, 150, 150, PathExtension.BrandThumbServer);
        if (!uploadingImgResult)
        {
            _logger.LogWarning("Brand creation failed: image upload failed. Title: {Title}", dto.Title);
            return false;
        }

        newBrand.ImageName = uniqueImgName;
        await _brandRepository.AddEntity(newBrand);
        await _brandRepository.SaveChanges();

        _logger.LogInformation("Brand created. BrandId: {BrandId}, Title: {Title}", newBrand.Id, newBrand.Title);
        return true;
    }

    public async Task<List<Brand>> GetAllBrands()
    {
        return await _brandRepository.GetQuery().ToListAsync();
    }

    #endregion

    #region Size

    public async Task<FilterSizeDto> FilterSizes(FilterSizeDto filter)
    {
        var query = _productSizeRepository.GetQuery().OrderByDescending(c => c.CreatedAt).AsQueryable();
        if (!string.IsNullOrEmpty(filter.Title))
        {
            query = query.Where(c => EF.Functions.Like(c.Title, $"%{filter.Title}%"));
        }
        if (!string.IsNullOrEmpty(filter.SizeType))
        {
            query = query.Where(c => EF.Functions.Like(c.SizeType, $"%{filter.SizeType}%"));
        }
        return new FilterSizeDto
        {
            Title = filter.Title,
            SizeType = filter.SizeType,
            Data = await query.ToListAsync()
        };

    }

    public async Task CreateSize(CreateSizeDto dto)
    {
        var newSize = new ProductSize
        {
            Title = dto.Title,
            SizeType = dto.SizeType,
            Order = dto.Order
        };
        await _productSizeRepository.AddEntity(newSize);
        await _productSizeRepository.SaveChanges();

        _logger.LogInformation("Product size created. SizeId: {SizeId}, Title: {Title}", newSize.Id, newSize.Title);
    }
    public async Task<List<ProductSize>> GetAllProductSizes()
    {
        return await _productSizeRepository.GetQuery().ToListAsync();
    }
    #endregion

    #region Product

    public async Task<CreateProductResult> CreateProduct(CreateProductDto dto)
    {
        int price = int.Parse(dto.BasePrice.Replace(",", ""));
        var newProduct = new Product
        {
            Title = dto.Title,
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            IsAvailable = dto.IsAvailable,
            Price = price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var newUniqueName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.MainImage.FileName);
        var res = dto.MainImage.AddImageToServer(newUniqueName, PathExtension.ProductImageServer, 150, 150, PathExtension.ProductImageThumbServer);
        if (res)
        {
            newProduct.MainImageName = newUniqueName;
        }
        else
        {
            _logger.LogWarning("Product creation failed: main image save failed. Title: {Title}", dto.Title);
            return CreateProductResult.SavingMainImageFailed;
        }
        if (dto.BrandId != 0 && await _brandRepository.GetEntityById(dto.BrandId) == null)
        {
            _logger.LogWarning("Product creation failed: brand not found. Title: {Title}, BrandId: {BrandId}", dto.Title, dto.BrandId);
            return CreateProductResult.BrandNotFound;
        }
        newProduct.BrandId = dto.BrandId == 0 ? null : dto.BrandId;

        await _productRepository.AddEntity(newProduct);
        await _productRepository.SaveChanges();

        var result = await _categoryService.AddProductSelectedCategories(dto.Categories, newProduct.Id);
        if (!result)
        {
            _logger.LogWarning("Product creation failed: category not found. ProductId: {ProductId}", newProduct.Id);
            return CreateProductResult.CategoryNotFound;
        }


        if (dto.ProductGalleries != null && dto.ProductGalleries.Any())
        {
            var galleryOrder = 1;
            foreach (var item in dto.ProductGalleries)
            {
                var galleryItem = new ProductGallery
                {
                    ProductId = newProduct.Id,
                    Order = galleryOrder
                };

                //Image
                var galleryImageName = Guid.NewGuid().ToString("N") + Path.GetExtension(item.FileName);
                item.AddImageToServer(galleryImageName, PathExtension.ProductGalleryServer, 150, 150, PathExtension.ProductGalleryThumbServer);
                galleryItem.ImageName = galleryImageName;

                await _productGalleryRepository.AddEntity(galleryItem);
                await _productGalleryRepository.SaveChanges();
                galleryOrder++;
            }
        }

        _logger.LogInformation("Product created. ProductId: {ProductId}, Title: {Title}, Price: {Price}", newProduct.Id, newProduct.Title, newProduct.Price);
        return CreateProductResult.Success;
    }

    public async Task<FilterProductDto> FilterProduct(FilterProductDto filter)
    {
        var query = _productRepository.GetQuery()
        .Include(p => p.ProductVariants)
        .Include(p => p.SelectedCategories)
        .ThenInclude(p => p.Category)
        .AsQueryable();

        switch (filter.ProductOrder)
        {
            case FilterProductOrder.Newest:
                query = query.OrderByDescending(d => d.CreatedAt);
                break;
            case FilterProductOrder.Oldest:
                query = query.OrderBy(d => d.CreatedAt);
                break;
            case FilterProductOrder.MostExpensive:
                query = query.OrderByDescending(d => d.Price);
                break;
            case FilterProductOrder.Cheapest:
                query = query.OrderBy(d => d.Price);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        switch (filter.ProductStatus)
        {
            case FilterProductStatus.All:
                break;
            case FilterProductStatus.Available:
                query = query.Where(d => d.IsAvailable);
                break;
            case FilterProductStatus.NotAvailable:
                query = query.Where(d => !d.IsAvailable);
                break;
            case FilterProductStatus.HasStockCount:
                query = query.Where(p => p.ProductVariants.Any(p => p.StockCount > 0));
                break;
            case FilterProductStatus.HasZeroStockCount:
                query = query.Where(p => !p.ProductVariants.Any(p => p.StockCount > 0));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        if (!string.IsNullOrEmpty(filter.Title))
        {
            query = query.Where(p => EF.Functions.Like(p.Title, $"%{filter.Title}%"));
        }

        if (!string.IsNullOrEmpty(filter.categoryUrl))
        {
            query = query.Where(d => d.SelectedCategories.Any(s => s.Category.Url == filter.categoryUrl));
        }

        if (filter.StartPrice != null)
        {
            query = query.Where(d => d.Price > filter.StartPrice);
        }

        if (filter.EndPrice != null)
        {
            query = query.Where(d => d.Price < filter.EndPrice);
        }

        if (filter is { StartPrice: not null, EndPrice: not null })
        {
            query = query.Where(d => d.Price > filter.StartPrice && d.Price < filter.EndPrice);
        }

        if (query.Any())
        {
            filter.MostPrice = query.OrderByDescending(d => d.Price).First().Price;
            filter.LeastPrice = query.OrderBy(d => d.Price).First().Price;
        }
        if (filter.CategoryId is > 0)
        {
            query = query.Where(d => d.SelectedCategories.Any(s => s.CategoryId == filter.CategoryId));
        }

        if (filter.ColorId is > 0)
        {
            query = query.Where(d => d.ProductVariants.Any(v => v.ColorId == filter.ColorId));
        }

        if (filter.BrandId is > 0)
        {
            query = query.Where(d => d.BrandId == filter.BrandId);
        }
        filter.Data = await query.ToListAsync();
        return filter;
    }

    public async Task<EditProductDto> GetEditProduct(long productId)
    {
        var data = await _productRepository.GetEntityById(productId);
        var model = new EditProductDto
        {
            Description = data.Description,
            IsAvailable = data.IsAvailable,
            Title = data.Title,
            ShortDescription = data.ShortDescription,
            ProductId = productId,
            BrandId = data.BrandId,
            Price = data.Price.ToString(),
            Categories = await _productSelectedCategoryRepository.GetQuery().Where(d => d.ProductId == productId)
        .Select(d => d.CategoryId).ToListAsync()
        };
        return model;
    }

    public async Task<EditProductResult> EditProduct(EditProductDto dto)
    {
        var product = await _productRepository.GetQuery().FirstOrDefaultAsync(d => d.Id == dto.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product edit failed: product not found. ProductId: {ProductId}", dto.ProductId);
            return EditProductResult.Error;
        }

        var oldValues = $"Title: {product.Title}, Price: {product.Price}, IsAvailable: {product.IsAvailable}, BrandId: {product.BrandId}";

        var convertedPrice = int.Parse(dto.Price.Replace(",", ""));

        product.Title = dto.Title;
        product.Description = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.IsAvailable = dto.IsAvailable;
        product.Price = convertedPrice;

        switch (dto.BrandId)
        {
            case > 0:
                {
                    var brand = await _brandRepository.GetQuery().FirstOrDefaultAsync(d => d.Id == dto.BrandId);
                    if (brand == null)
                    {
                        _logger.LogWarning("Product edit failed: brand not found. ProductId: {ProductId}, BrandId: {BrandId}", dto.ProductId, dto.BrandId);
                        return EditProductResult.BrandNotFound;
                    }
                    product.BrandId = dto.BrandId;
                    break;
                }
            case 0:
                {
                    product.BrandId = null;
                    break;
                }
        }

        await _categoryService.RemoveProductSelectedCategories(dto.ProductId);
        var addCategoryResult = await _categoryService.AddProductSelectedCategories(dto.Categories, dto.ProductId);
        if (!addCategoryResult)
        {
            _logger.LogWarning("Product edit failed: category not found. ProductId: {ProductId}", dto.ProductId);
            return EditProductResult.CategoryNotFound;
        }



        if (dto.MainImage != null)
        {
            var newUniqueImageName = Guid.NewGuid().ToString("N") + Path.GetExtension(dto.MainImage.FileName);
            var res = dto.MainImage.AddImageToServer(newUniqueImageName, PathExtension.ProductImageServer, 150, 150, PathExtension.ProductImageThumbServer, product.MainImageName);
            if (res)
            {
                product.MainImageName = newUniqueImageName;
            }
            else
            {
                _logger.LogWarning("Product edit failed: image save failed. ProductId: {ProductId}", dto.ProductId);
                return EditProductResult.ImageNotSaved;
            }

        }


        _productRepository.UpdateEntity(product);
        await _productRepository.SaveChanges();

        var newValues = $"Title: {product.Title}, Price: {product.Price}, IsAvailable: {product.IsAvailable}, BrandId: {product.BrandId}";

        _logger.LogInformation(
            "Product updated. ProductId: {ProductId}. Old values -> {OldValues}. New values -> {NewValues}",
            dto.ProductId, oldValues, newValues);

        return EditProductResult.Success;
    }

    public async Task<ProductDetailDto> GetProductDetail(long productId)
    {
        var data = await _productRepository.GetEntityById(productId);
        var model = new ProductDetailDto
        {
            Id = data.Id,
            Title = data.Title,
            IsDeleted = data.IsAvailable,
            LastUpdateDate = data.UpdatedAt,
            CreateDate = data.CreatedAt,
            Description = data.Description,
            MainImageName = data.MainImageName,
            IsAvailable = data.IsAvailable,
            ShortDescription = data.ShortDescription,
            Price = data.Price,
            ProductSelectedBrand = await _productRepository.GetQuery().Include(p => p.Brand).Where(p => p.Id == productId).Select(p => p.Brand).FirstOrDefaultAsync(),
            ProductVariants = await _productVariantRepository.GetQuery().Where(d => d.ProductId == productId).ToListAsync(),
            SelectedCategories = await _productSelectedCategoryRepository.GetQuery().Include(c => c.Category).Where(d => d.ProductId == productId).ToListAsync(),
            ProductGalleries = await _productGalleryRepository.GetQuery().Where(d => d.ProductId == productId).ToListAsync(),
            ProductFeatures = await _productFeatureRepository.GetQuery().Where(d => d.ProductId == productId).ToListAsync(),
        };

        return model;
    }

    public async Task CreateProductVariant(CreateProductVariantDto dto)
    {
        foreach (var item in dto.Variantitems)
        {
            var color = await _productColorRepository.GetEntityById(item.ColorId);
            var productVariant = new ProductVariant
            {
                ProductId = dto.ProductId,
                ColorId = item.ColorId,
                SizeId = item.SizeId,
                StockCount = item.StockCount,
                ColorTitle = color.Title,
                ColorCode = color.ColorCode,
                SizeTitle = await _productSizeRepository.GetQuery().Where(s => s.Id == item.SizeId).Select(s => s.Title).SingleOrDefaultAsync()
            };
            await _productVariantRepository.AddEntity(productVariant);
        }
        await _productVariantRepository.SaveChanges();

        _logger.LogInformation("Product variants created. ProductId: {ProductId}, Count: {Count}", dto.ProductId, dto.Variantitems.Count);
    }

    public async Task CreateProductFeature(CreateProductFeatureDto dto)
    {
        foreach (var item in dto.FeatureItems)
        {
            var productFeature = new ProductFeature
            {
                ProductId = dto.ProductId,
                Title = item.Title,
                Value = item.Value,
                Order = item.Order
            };
            await _productFeatureRepository.AddEntity(productFeature);
        }
        await _productFeatureRepository.SaveChanges();

        _logger.LogInformation("Product features created. ProductId: {ProductId}, Count: {Count}", dto.ProductId, dto.FeatureItems.Count);
    }
    public async Task<List<ProductItemDto>> GetNewestProducts()
    {
        var products = await _cache.GetOrCreateAsync("NewProducts", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

                var newProducts = await _productRepository.GetQuery()
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(10)
                    .Select(p => new ProductItemDto
                    {
                        ProductId = p.Id,
                        Title = p.Title,
                        Image = p.MainImageName,
                        Price = p.Price,
                    })
                    .ToListAsync();

                return newProducts;
            });
        return products ?? new List<ProductItemDto>();
    }


    #endregion
}