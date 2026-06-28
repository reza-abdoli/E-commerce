using Data.Entities;

namespace Data.Entities
{
    public class ProductGallery : BaseEntity
    {
        public long ProductId { get; set; }
        public string ImageName { get; set; }
        public int Order { get; set; }
    }
}
