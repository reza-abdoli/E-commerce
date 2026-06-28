using Data.Entities;

namespace Data.Entities
{
    public class ProductSelectedCategory : BaseEntity
    {
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public Product Product { get; set; }
        public ProductCategory Category { get; set; }
    }
}
