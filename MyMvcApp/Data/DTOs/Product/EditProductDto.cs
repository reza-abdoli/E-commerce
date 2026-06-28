using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Data.DTOs.Product
{
    public class EditProductDto
    {
        public long ProductId { get; set; }
        public string Title { get; set; }
        public bool IsAvailable { get; set; }
        public IFormFile? MainImage { get; set; }
        public string Price { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public long? BrandId { get; set; }
        public List<long> Categories { get; set; }
    }
    public enum EditProductResult
    {
        Success,
        Error,
        ImageNotSaved,
        BrandNotFound,
        CategoryNotFound
    }
}
