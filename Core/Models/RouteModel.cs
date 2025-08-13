namespace Core.Models;

public class RouteModel
{
    public IEnumerable<PathModel> Paths { get; set; } = new List<PathModel>();
    public int Distance {get; set;}
}