namespace Data.DTOs.Product;

public class CreateProductFeatureDto
{
    public long ProductId { get; set; }
    public List<CreateFeatureItemDTo> FeatureItem { get; set; }
}
