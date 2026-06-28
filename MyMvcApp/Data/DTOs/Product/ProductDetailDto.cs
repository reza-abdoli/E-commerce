using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Entities;

namespace Data.DTOs.Product
{
    public class ProductDetailDto
    {
        public long Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Title { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string MainImageName { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public Brand? ProductSelectedBrand { get; set; }
        public List<ProductSelectedCategory> SelectedCategories { get; set; }
        public List<ProductVariant> ProductVariants { get; set; }
        public List<ProductGallery> ProductGalleries { get; set; }
        public List<ProductFeature> ProductFeatures { get; set; }
    }
}
