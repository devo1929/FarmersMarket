namespace Core.Models;

public class OrderCreateModel
{
    public IEnumerable<OrderProductCreateModel> Products { get; set; } =  new List<OrderProductCreateModel>();
}