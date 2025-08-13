namespace Core.Models;

public class RouteModel
{
    public IEnumerable<PathModel> Paths { get; set; }
    public int Distance {get; set;}
}