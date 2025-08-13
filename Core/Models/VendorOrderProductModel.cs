using Core.Enums;

namespace Core.Models;

public class VendorOrderProductModel
{
    public long Id { get; set; }
    public long VendorOrderId { get; set; }
    public long ProductId { get; set; }
    public long Units { get; set; }
    public OrderStatusEnum Status { get; set; }
    public string? Notes { get; set; }
}