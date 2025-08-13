using Core.Entities;
using Mock;

namespace WebAPI.Database;

public class OrderRepository
{
    public async Task<OrderEntity> CreateAsync(OrderEntity order) => 
        MockDatabase.AddOrder(order);

    public async Task<IEnumerable<OrderEntity>> GetAllAsync() =>
        MockDatabase.Orders;
}