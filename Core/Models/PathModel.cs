namespace Core.Models;

public class PathModel(LocationModel start, LocationModel end) : IEquatable<PathModel>
{
    public LocationModel Start { get; } = start;
    public LocationModel End { get; } = end;

    public bool Equals(PathModel? other)
    {
        if (other == null)
            return false;
        return Start.X == other.Start.X && Start.Y == other.Start.Y && End.X == other.End.X && End.Y == other.End.Y;
    }
}