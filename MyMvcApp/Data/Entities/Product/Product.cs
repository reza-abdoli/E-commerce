using Data.Entities;

namespace Data.Entities
{
    public class Product : BaseEntity
    {
        #region Properties
        public string Title { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string MainImageName { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public long? BrandId { get; set; }
        public Brand? Brand { get; set; }
        
        
        #endregion

        #region Relations
        public ICollection<ProductSelectedCategory> SelectedCategories { get; set; } = new List<ProductSelectedCategory>();
        //public ICollection<ProductComment>? ProductComments { get; set; } = new List<ProductComment>();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductGallery> ProductGalleries { get; set; } = new List<ProductGallery>();
        public ICollection<ProductFeature> ProductFeatures { get; set; } = new List<ProductFeature>();
        #endregion
    }
}
