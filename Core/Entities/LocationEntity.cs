namespace Core.Entities;

public class LocationEntity
{
    public int Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public LocationEntity()
    {
    }

    public LocationEntity(int x, int y)
    {
        X = x;
        Y = y;
    }
}