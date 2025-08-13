using Core.Enums;

namespace Core.Entities;

public class VendorEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public VendorStatusEnum Status { get; set; }

    public virtual ICollection<VendorLocationEntity> VendorBounds { get; set; } = new List<VendorLocationEntity>();

    /// <summary>
    /// These are all the locations/points that make up the bounds of a given vendor in the market.
    /// It's like drawing a box around the vendor.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<LocationEntity> GetAllLocations()
    {
        var boundedLocations = VendorBounds.Select(vl => vl.Location).ToList();
        var allLocations = new List<LocationEntity>();
        for (var i = 0; i < boundedLocations.Count; i++)
        {
            var loc1 = boundedLocations[i];
            var loc2 = i == boundedLocations.Count - 1 ? boundedLocations[0] : boundedLocations[i + 1];
            if (loc1.X == loc2.X)
            {
                var minY = Math.Min(loc1.Y, loc2.Y);
                var maxY = Math.Max(loc1.Y, loc2.Y);
                for (var y = minY; y <= maxY; y++)
                    allLocations.Add(new LocationEntity(loc1.X, y));
            }
            else if (loc1.Y == loc2.Y)
            {
                var minX = Math.Min(loc1.X, loc2.X);
                var maxX = Math.Max(loc1.X, loc2.X);
                for (var x = minX; x <= maxX; x++)
                    allLocations.Add(new LocationEntity(x, loc1.Y));
            }
        }

        return allLocations;
    }

    /// <summary>
    /// This builds a list of all possible locations a product could exist in a given Vendor bounds.
    /// That is all "side" points, not corners.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<LocationEntity> GetPossibleProductLocations() =>
        GetAllLocations().Where(l => VendorBounds.All(vl => !vl.Location.Equals(l)));
}