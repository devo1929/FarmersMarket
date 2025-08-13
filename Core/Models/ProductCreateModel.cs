using Core.Enums;

namespace Core.Models;

public class ProductCreateModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Units { get; set; }
    public decimal PricePerUnit { get; set; }
    public ProductStatusEnum Status { get; set; }
}