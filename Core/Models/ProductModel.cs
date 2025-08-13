namespace Core.Models;

public class ProductModel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public VendorModel Vendor { get; set; } = null!;
    public int Units { get; set; }
    public decimal PricePerUnit { get; set; }
    public LocationModel Location { get; set; } = null!;
}