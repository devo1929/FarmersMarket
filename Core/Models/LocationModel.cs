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

    public bool Equals(LocationModel? other)
    {
        if (other == null)
            return false;
        return other.X == X && other.Y == Y;
    }
}