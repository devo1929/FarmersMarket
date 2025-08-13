using Core.Enums;

namespace Core.Entities;

public class VendorEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public VendorStatusEnum Status { get; set; }

    public virtual ICollection<VendorLocationEntity> VendorLocations { get; set; } = new List<VendorLocationEntity>();

    public IEnumerable<LocationEntity> GetPossibleProductLocations()
    {
        var locations = VendorLocations.Select(vl => vl.Location).ToList();
        var possibleProductLocations = new List<LocationEntity>();
        for (var i = 0; i < locations.Count - 1; i++)
        {
            var loc1 = locations[i];
            var loc2 = locations[i + 1];
            if (loc1.X == loc2.X)
            {
                var minY = Math.Min(loc1.Y, loc2.Y);
                var maxY = Math.Max(loc1.Y, loc2.Y);
                for (var y = minY + 1; y < maxY; y++)
                    possibleProductLocations.Add(new LocationEntity(loc1.X, y));
            }
            else if (loc1.Y == loc2.Y)
            {
                var minX = Math.Min(loc1.X, loc2.X);
                var maxX = Math.Max(loc1.X, loc2.X);
                for (var x = minX + 1; x < maxX; x++)
                    possibleProductLocations.Add(new LocationEntity(x, loc1.Y));
            }
        }

        return possibleProductLocations;
    }
}