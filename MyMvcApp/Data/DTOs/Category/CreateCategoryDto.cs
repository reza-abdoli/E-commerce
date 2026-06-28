using Microsoft.AspNetCore.Http;

namespace Data.DTOs.Category
{
    public class CreateCategoryDto
    {
        public long? ParentId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public int Order { get; set; }
        public bool ShowInHome { get; set; }
        public IFormFile MainImage { get; set; }
    }
}
