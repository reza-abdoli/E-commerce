using Microsoft.AspNetCore.Http;

namespace Data.DTOs.Product;

public class CreateProductDto
{
    public string Title { get; set; }
    public long BrandId { get; set; }
    public bool IsAvailable { get; set; }
    public string BasePrice { get; set; } 
    public IFormFile MainImage { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public List<long> Categories { get; set; }
    public List<IFormFile>? ProductGalleries { get; set; }
}

public enum CreateProductResult
{
    Success,
    Error,
    SavingMainImageFailed,
    BrandNotFound,
    CategoryNotFound
}
