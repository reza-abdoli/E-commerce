namespace Data.DTOs.Product
{
    public class CreateProductVariantDto
    {
        public long ProductId { get; set; }
        public List<CreateVariantItemDto> Variantitems { get; set; }
    }
}
