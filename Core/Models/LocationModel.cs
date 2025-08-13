namespace Core.Models;

public class LocationModel : IEquatable<LocationModel>
{
    public int X { get; set; }
    public int Y { get; set; }

    public LocationModel()
    {
    }

    public LocationModel(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(LocationModel? a, LocationModel? b)
    {
        if (a == null || b == null)
            return false;
        return a.X == b.X && a.Y == b.Y;
    }

    public int GetHashCode(LocationModel obj)
    {
        var hash = 7;
        hash = 3 * hash + X;
        hash = 3 * hash + Y;
        return hash;
    }

    public bool Equals(LocationModel? other)
    {
        if (other == null)
            return false;
        return other.X == X && other.Y == Y;
    }
}