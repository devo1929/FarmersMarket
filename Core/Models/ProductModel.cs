namespace Core.Models;

public class ProductModel
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public VendorModel Vendor { get; set; }
}