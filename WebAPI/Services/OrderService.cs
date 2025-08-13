using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Models;
using Mock;
using WebAPI.Database;

namespace WebAPI.Services;

public class OrderService(IMapper mapper, OrderRepository orderRepository)
{
    public async Task<OrderModel> CreateAsync(OrderCreateModel model)
    {
        var productGroups = model.Products.GroupBy(p => MockData.Products.Single(product => product.Id == p.Id).VendorId);
        var orderEntity = new OrderEntity
        {
            VendorOrders = productGroups.Select(group => new VendorOrderEntity
            {
                Vendor = MockData.Vendors.Single(vendor => vendor.Id == group.Key),
                Products = group.Select(orderProduct => new VendorOrderProductEntity
                {
                    Product = MockData.Products.Single(product => product.Id == orderProduct.Id),
                    Status = VendorOrderProductStatusEnum.Pending,
                    Units = orderProduct.Units
                }).ToList()
            }).ToList()
        };
        return mapper.Map<OrderModel>(await orderRepository.CreateAsync(orderEntity));
    }

    public async Task<IEnumerable<OrderModel>> GetAllAsync() =>
        mapper.Map<IEnumerable<OrderModel>>(await orderRepository.GetAllAsync());

    public async Task<RouteModel> GetRouteAsync(long orderId)
    {
        return new RouteModel
        {
            Paths = new List<PathModel>()
        };
    }
}