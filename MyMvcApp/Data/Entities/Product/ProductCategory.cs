using Data.Entities;

namespace Data.Entities
{
    public class ProductCategory : BaseEntity
    {
        public long? ParentId { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string MainImage { get; set; }
        public bool IsActive { get; set; }
        public bool ShowInHome { get; set; }
        public int Order { get; set; }
        public ProductCategory Parent { get; set; }
        public ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
        public ICollection<ProductSelectedCategory> ProductSelectedCategories { get; set; }
    }
}
