using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.DTOs;
using Data.DTOs.Product;
using Data.Entities;

namespace Application.Services.Interface;

public interface IProductService : IAsyncDisposable
{
    #region Color
    Task<FilterColorDto> FilterColors(FilterColorDto filter);
    Task CreateColor(CreateColorDTo color);
    #endregion

    #region Size
    Task<FilterSizeDto> FilterSizes(FilterSizeDto filter);
    Task CreateSize(CreateSizeDto dto);
    #endregion

    #region Brand
    Task<FilterBrandDto> FilterBrands(FilterBrandDto filter);
    Task<bool> CreateBrand(CreateBrandDto color);

    Task<List<Brand>> GetAllBrands();
    #endregion

    #region Product
    Task<CreateProductResult> CreateProduct(CreateProductDto dto);
    Task<FilterProductDto> FilterProduct(FilterProductDto filter);
    Task<EditProductDto> GetEditProduct(long productId);
    Task<EditProductResult> EditProduct(EditProductDto dto);

    Task<ProductDetailDto> GetProductDetail(long productId);


    #endregion
}