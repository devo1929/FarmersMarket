namespace Core.Models;

public class VendorOrderModel
{
    public long Id { get; set; }
    public long OrderId { get; set; }
    public VendorModel Vendor { get; set; } = null!;

    public IEnumerable<VendorOrderProductModel> Products { get; set; } = new List<VendorOrderProductModel>();
}