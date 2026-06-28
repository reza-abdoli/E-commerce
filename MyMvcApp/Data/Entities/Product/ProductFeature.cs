using Data.Entities;

namespace Data.Entities
{
    public class ProductFeature : BaseEntity
    {
        public long ProductId { get; set; }
        public string Title { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
        public Product Product { get; set; }
    }
}
