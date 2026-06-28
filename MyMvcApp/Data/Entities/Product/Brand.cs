

using Data.Entities;

namespace Data.Entities
{
    public class Brand : BaseEntity
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImageName { get; set; }
        public int Order { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
