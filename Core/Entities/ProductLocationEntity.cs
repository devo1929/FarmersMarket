namespace Core.Entities;

public class ProductLocationEntity
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public long LocationId { get; set; }

    public virtual ProductEntity Product { get; set; } = null!;
    public virtual LocationEntity Location { get; set; } = null!;
}