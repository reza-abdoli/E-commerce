using Microsoft.AspNetCore.Http;

namespace Data.DTOs.Product
{
    public class CreateBrandDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public IFormFile Image { get; set; }
        public int Order { get; set; }
    }
}
