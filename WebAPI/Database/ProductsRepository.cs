using Core.Entities;
using Core.Enums;
using Mock;

namespace WebAPI.Database;

public class ProductsRepository
{
    public async Task<IEnumerable<ProductEntity>> GetAllAsync() =>
        MockData.Products.Where(p => p.Status == ProductStatusEnum.Active);

    public async Task<ProductEntity> GetAsync(long id) =>
        MockData.Products.Single(p => p.Id == id);

    public async Task<ProductEntity> CreateAsync(ProductEntity product)
    {
        MockData.AddProduct(product);
        return product;
    }

    public async Task DeleteAsync(long productId) =>
        MockData.Products.Remove(MockData.Products.Single(p => p.Id == productId));
}