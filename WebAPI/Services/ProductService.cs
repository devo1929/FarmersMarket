using AutoMapper;
using Core.Entities;
using Core.Models;
using WebAPI.Database;

namespace WebAPI.Services;

public class ProductService(IMapper mapper, ProductsRepository productRepository, RouteService routeService)
{
    public async Task<IEnumerable<ProductModel>> GetAllAsync() =>
        mapper.Map<IEnumerable<ProductModel>>(await productRepository.GetAllAsync());

    public async Task<ProductModel> CreateAsync(ProductCreateModel model)
    {
        var productEntity = mapper.Map<ProductEntity>(model);
        return mapper.Map<ProductModel>(await productRepository.CreateAsync(productEntity));
    }

    public async Task<ProductModel> UpdateAsync(ProductUpdateModel model)
    {
        var productEntity = await productRepository.GetAsync(model.Id);
        productEntity.Name = model.Name;
        productEntity.Description = model.Description;
        productEntity.Units = model.Units;
        productEntity.PricePerUnit = model.PricePerUnit;
        productEntity.Status = model.Status;
        return mapper.Map<ProductModel>(productEntity);
    }

    public async Task DeleteAsync(long productId)
        => await productRepository.DeleteAsync(productId);

    public async Task<ProductModel> GetAsync(long productId)
        => mapper.Map<ProductModel>(await productRepository.GetAsync(productId));

    public async Task<RouteModel> GetRouteAsync(long productId)
    {
        var product = await productRepository.GetAsync(productId);
        return await routeService.GetRouteAsync(product);
    }
}