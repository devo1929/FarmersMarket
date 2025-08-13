using Core.Enums;

namespace Core.Entities;

public class OrderEntity
{
    public long Id { get; set; }
    public Guid ReferenceId { get; set; }
    public OrderStatusEnum Status { get; set; }

    public virtual ICollection<VendorOrderEntity> VendorOrders { get; set; } = new List<VendorOrderEntity>();
}