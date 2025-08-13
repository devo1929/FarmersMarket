using Core.Entities;
using Core.Enums;
using Mock;

namespace WebAPI.Database;

public class ProductsRepository
{
    public async Task<IEnumerable<ProductEntity>> GetAllAsync() =>
        MockDatabase.Products.Where(p => p.Status == ProductStatusEnum.Active);

    public async Task<ProductEntity> GetAsync(long id) =>
        MockDatabase.Products.Single(p => p.Id == id);

    public async Task<ProductEntity> CreateAsync(ProductEntity product)
    {
        MockDatabase.AddProduct(product);
        return product;
    }

    public async Task DeleteAsync(long productId) =>
        MockDatabase.Products.Remove(MockDatabase.Products.Single(p => p.Id == productId));
}