using Microsoft.AspNetCore.Http;

namespace Data.DTOs.Product;

public class CreateGalleryDto
{
    public long ProductId { get; set; }
    public IFormFile ImageName { get; set; }
    public int Order { get; set; }
}
