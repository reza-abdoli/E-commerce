using Data.Entities;

namespace Data.Entities
{
    public class ProductVariant : BaseEntity
    {
        public long ProductId { get; set; }
        public long ColorId { get; set; }
        // public int Price { get; set; }
        public long SizeId { get; set; }
        public int StockCount { get; set; }
        public string ColorTitle { get; set; }
        public string ColorCode { get; set; }
        public string SizeTitle { get; set; }
        public Product Product { get; set; }
    }
}
