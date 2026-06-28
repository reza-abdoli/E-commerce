using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class ProductSize : BaseEntity
    {
        public string Title { get; set; }
        public string SizeType { get; set; }
        public int Order { get; set; }
        public ICollection<ProductVariant> ProductVariants { get; set; }

    }
}