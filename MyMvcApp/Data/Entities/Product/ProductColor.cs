using Data.Entities;

namespace Data.Entities {
    public class ProductColor : BaseEntity
    {
        public string Title { get; set; }
        public string ColorCode { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; }
    }
}
