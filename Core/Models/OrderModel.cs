using Core.Enums;

namespace Core.Models;

public class OrderModel
{
    public long Id { get; set; }
    public Guid ReferenceId { get; set; }
    public OrderStatusEnum Status { get; set; }
    
    public IEnumerable<VendorOrderModel> VendorOrders { get; set; } = new List<VendorOrderModel>();
}