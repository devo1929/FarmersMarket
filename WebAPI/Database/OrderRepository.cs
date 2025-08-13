using Core.Entities;
using Mock;

namespace WebAPI.Database;

public class OrderRepository
{
    public async Task<OrderEntity> CreateAsync(OrderEntity order) => 
        MockData.AddOrder(order);

    public async Task<IEnumerable<OrderEntity>> GetAllAsync() =>
        MockData.Orders;
}