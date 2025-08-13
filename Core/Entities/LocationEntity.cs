namespace Core.Entities;

public class LocationEntity : IEquatable<LocationEntity>
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

    public bool Equals(LocationEntity? other)
    {
        if (other == null) return false;
        return other.X == X && other.Y == Y;
    }
}