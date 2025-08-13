using AutoMapper;
using Core.Entities;
using Core.Models;
using Dijkstra.NET.Graph;
using Dijkstra.NET.ShortestPath;
using Mock;
using WebAPI.Database;

namespace WebAPI.Services;

public class RouteService(IMapper mapper, VendorRepository vendorRepository)
{
    private static readonly LocationModel BaseLocation = new();

    public async Task<RouteModel> GetRouteAsync(ProductEntity product)
    {
        var productLocation = mapper.Map<LocationModel>(product.Location);
        var graph = await BuildGraphAsync();
        var result = graph.GetResult(BaseLocation, productLocation);
        var nodes = result.GetPath().ToList();
        var paths = new List<PathModel>();
        for (var i = 0; i < nodes.Count - 1; i++)
        {
            var node1 = graph[nodes[i]];
            var node2 = graph[nodes[i + 1]];
            paths.Add(new PathModel(node1.Item, node2.Item));
        }

        return new RouteModel
        {
            Paths = paths,
            Distance = result.Distance
        };
    }

    private async Task<RouteGraph> BuildGraphAsync()
    {
        var vendors = await vendorRepository.GetAllAsync();
        var graph = new RouteGraph();

        for (var x = 0; x <= MockDatabase.MaxX; x++)
        for (var y = 0; y <= MockDatabase.MaxY; y++)
            graph.Add(new LocationModel(x, y));

        foreach (var vendor in vendors)
        {
            var locations = vendor.VendorLocations.Select(vl => vl.Location).ToList();
            for (var i = 0; i < locations.Count; i++)
            {
                var loc1 = locations[i];
                var loc2 = i == locations.Count - 1 ? locations[0] : locations[i + 1];
                if (loc1.X == loc2.X)
                {
                    var minY = Math.Min(loc1.Y, loc2.Y);
                    var maxY = Math.Max(loc1.Y, loc2.Y);
                    for (var y = minY; y < maxY; y++)
                        graph.Connect(new LocationModel(loc1.X, y), new LocationModel(loc1.X, y + 1));
                }
                else if (loc1.Y == loc2.Y)
                {
                    var minX = Math.Min(loc1.X, loc2.X);
                    var maxX = Math.Max(loc1.X, loc2.X);
                    for (var x = minX; x < maxX; x++)
                        graph.Connect(new LocationModel(x, loc1.Y), new LocationModel(x + 1, loc1.Y));
                }
            }
        }

        return graph;
    }

    private class RouteGraph : Graph<LocationModel, string>
    {
        private Dictionary<LocationModel, uint> Nodes { get; } = new(new LocationEqualityComparer());

        public void Add(LocationModel loc)
        {
            var key = AddNode(loc);
            Nodes[loc] = key;
        }

        public void Connect(LocationModel loc1, LocationModel loc2)
        {
            var key1 = Nodes[loc1];
            var key2 = Nodes[loc2];
            Nodes[loc1] = key1;
            Nodes[loc2] = key2;
            Connect(key1, key2, CalculateCost(loc1, loc2), string.Empty);
        }

        public ShortestPathResult GetResult(LocationModel a, LocationModel b) =>
            this.Dijkstra(Nodes[a], Nodes[b]);

        public int CalculateCost(LocationModel start, LocationModel end) => (int)Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
    }

    private class LocationEqualityComparer : IEqualityComparer<LocationModel>
    {
        public bool Equals(LocationModel? a, LocationModel? b)
        {
            if (a == null || b == null)
                return false;
            return a.X == b.X && a.Y == b.Y;
        }

        public int GetHashCode(LocationModel obj)
        {
            var hash = 7;
            hash = 3 * hash + obj.X;
            hash = 3 * hash + obj.X;
            return hash;
        }
    }
}