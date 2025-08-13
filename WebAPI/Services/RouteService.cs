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

    /// <summary>
    /// This builds the "graph" that is the farmers' market grid. With all grid points created, it then "connects" all points
    /// around each vendor space in the grid. These connections are paths that can be traveled in the market.. 
    /// </summary>
    /// <returns></returns>
    private async Task<RouteGraph> BuildGraphAsync()
    {
        var vendors = await vendorRepository.GetAllAsync();
        var graph = new RouteGraph();

        for (var x = 0; x <= MockDatabase.MaxX; x++)
        for (var y = 0; y <= MockDatabase.MaxY; y++)
            graph.Add(new LocationModel(x, y));

        foreach (var vendor in vendors)
        {
            var locations = vendor.GetAllLocations().ToList();
            for (var i = 0; i < locations.Count; i++)
            {
                var loc1 = locations[i];
                var loc2 = i == locations.Count - 1 ? locations[0] : locations[i + 1];
                graph.Connect(new LocationModel(loc1.X, loc1.Y), new LocationModel(loc2.X, loc2.Y));
            }
        }

        return graph;
    }

    /// <summary>
    /// This is a simple graph to help extend the Dijkstra algorithm library for our needs.
    /// </summary>
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

        /// <summary>
        /// This calculates the "cost" of a path by using the pythagorean theorem. In the current example, we're using a simple grid of cells of size 1.
        /// However, this would allow us to calculate the cost of a given path if a vendor had a non-rectangular bounds within the market, thus allowing
        /// the customer to travel diagonally.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static int CalculateCost(LocationModel start, LocationModel end) => (int)Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
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