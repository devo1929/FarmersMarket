using AutoMapper;
using Core.Entities;
using Core.Models;

namespace Core.Classes;

public class DefaultProfile : Profile
{
    public DefaultProfile()
    {
        CreateMap<VendorEntity, VendorModel>();
        CreateMap<ProductEntity, ProductModel>();
        CreateMap<ProductCreateModel, ProductEntity>();
        CreateMap<LocationEntity, LocationModel>();
        CreateMap<OrderEntity, OrderModel>();
        CreateMap<VendorOrderEntity, VendorOrderModel>();
        CreateMap<VendorOrderProductEntity, VendorOrderProductModel>();
    }
}