using Bogus;
using Core.Entities;
using Core.Enums;

namespace Mock;

public static class MockDatabase
{
    public const int MaxX = 14;

    public const int MaxY = 8;

    /***********************/
    private static int _vendorId;
    private static int _productId;
    private static int _userId;
    private static int _orderId;
    private static int _vendorLocationId;
    private static int _vendorOrderId;
    private static int _vendorOrderProductId;

    public static void Init()
    {
        Locations = GenerateLocations();
        Users = UserFaker.Generate(10);
        Vendors = VendorFaker.Generate(14);
        Products = ProductFaker.Generate(50);
        Orders = OrderFaker.Generate(5);
    }

    private static readonly Faker<VendorEntity> VendorFaker = new Faker<VendorEntity>()
        .RuleFor(v => v.Id, f => ++_vendorId)
        .RuleFor(v => v.Name, f => f.Company.CompanyName())
        .RuleFor(v => v.Description, f => f.Company.CatchPhrase())
        .RuleFor(v => v.Status, f => VendorStatusEnum.Active)
        .RuleFor(v => v.VendorBounds, (_, v) => GetVendorLocationsForVendor(v));

    private static readonly Faker<ProductEntity> ProductFaker = new Faker<ProductEntity>()
        .RuleFor(p => p.Id, f => ++_productId)
        .RuleFor(p => p.Name, f => f.Commerce.ProductName())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.Status, f => ProductStatusEnum.Active)
        .RuleFor(p => p.Units, f => f.Random.Int(5, 50))
        .RuleFor(p => p.PricePerUnit, f => f.Random.Decimal(5.0m, 50.0m))
        .Rules((f, p) =>
        {
            var vendor = f.Random.ListItem(Vendors);
            p.VendorId = vendor.Id;
            p.Vendor = vendor;
        })
        .RuleFor(p => p.Location, (_, p) => GenerateRandomLocationForProduct(p));

    private static readonly Faker<UserEntity> UserFaker = new Faker<UserEntity>()
        .RuleFor(p => p.Id, f => ++_userId)
        .RuleFor(p => p.FirstName, f => f.Person.FirstName)
        .RuleFor(p => p.LastName, f => f.Person.LastName)
        .RuleFor(p => p.Email, f => f.Person.Email);

    private static readonly Faker<OrderEntity> OrderFaker = new Faker<OrderEntity>()
        .RuleFor(o => o.Id, f => ++_orderId)
        .RuleFor(o => o.Status, f => f.Random.Enum<OrderStatusEnum>())
        .RuleFor(o => o.ReferenceId, _ => Guid.NewGuid())
        .RuleFor(o => o.VendorOrders, GenerateVendorOrders);


    public static List<UserEntity> Users;
    public static List<VendorEntity> Vendors;
    public static List<ProductEntity> Products;
    public static List<OrderEntity> Orders;
    private static List<LocationEntity> Locations;

    private static List<LocationEntity> GenerateLocations()
    {
        var locations = new List<LocationEntity>();
        var id = 0;
        for (var x = 0; x <= MaxX; x++)
        {
            for (var y = 0; y <= MaxY; y++)
            {
                locations.Add(new LocationEntity
                {
                    Id = ++id,
                    X = x,
                    Y = y
                });
            }
        }

        return locations;
    }

    private static IEnumerable<VendorOrderEntity> GenerateVendorOrders(Faker faker, OrderEntity order) =>
        faker.Random.ListItems(Vendors).Select(v => new VendorOrderEntity
        {
            Id = ++_vendorOrderId,
            Order = order,
            OrderId = order.Id,
            Vendor = v,
            VendorId = v.Id,
            Notes = faker.Lorem.Sentence(5),
            Status = OrderStatusEnum.Complete,
            Products = Products.Where(p => p.Vendor.Id == v.Id).Select(p => new VendorOrderProductEntity
            {
                Id = ++_vendorOrderProductId,
                VendorOrderId = _vendorOrderId,
                Product = p,
                ProductId = p.Id,
                Notes = faker.Lorem.Sentence(3),
                Units = faker.Random.Int(1, 10),
                Status = faker.PickRandom<VendorOrderProductStatusEnum>()
            }).ToList()
        }).ToList();

    private static LocationEntity GenerateRandomLocationForProduct(ProductEntity product)
    {
        var possibleProductLocations = product.Vendor.GetPossibleProductLocations().ToList();
        var random = new Random();
        return possibleProductLocations[random.Next(0, possibleProductLocations.Count)];
    }

    private static List<VendorLocationEntity> GetVendorLocationsForVendor(VendorEntity v) =>
        GetLocationsForVendorId(v.Id).Select(l => new VendorLocationEntity
        {
            Id = ++_vendorLocationId,
            Vendor = v,
            Location = l
        }).ToList();

    /// <summary>
    /// To strictly define bounds of each vendor based on provided diagram.
    /// </summary>
    /// <param name="vendorId"></param>
    /// <returns></returns>
    private static List<LocationEntity> GetLocationsForVendorId(long vendorId) =>
        vendorId switch
        {
            1 => //A
                GetLocationsForBounds(0, 0, 4, 4),
            2 => //B
                GetLocationsForBounds(4, 0, 8, 2),
            3 => //C
                GetLocationsForBounds(10, 0, 12, 2),
            4 => //D
                GetLocationsForBounds(12, 0, 14, 2),
            5 => //E
                GetLocationsForBounds(10, 2, 14, 4),
            6 => //F
                GetLocationsForBounds(8, 0, 10, 6),
            7 => //G
                GetLocationsForBounds(6, 2, 8, 6),
            8 => //H
                GetLocationsForBounds(4, 2, 6, 6),
            9 => //I
                GetLocationsForBounds(0, 4, 4, 6),
            10 => //J
                GetLocationsForBounds(0, 6, 2, 8),
            11 => //K
                GetLocationsForBounds(2, 6, 6, 8),
            12 => //L
                GetLocationsForBounds(6, 6, 12, 8),
            13 => //M
                GetLocationsForBounds(10, 4, 14, 6),
            14 => //N
                GetLocationsForBounds(12, 6, 14, 8),
            _ => throw new Exception("Invalid vendor ID")
        };

    private static List<LocationEntity> GetLocationsForBounds(int x1, int y1, int x2, int y2) =>
    [
        GetLocationForPoint(x1, y1),
        GetLocationForPoint(x1, y2),
        GetLocationForPoint(x2, y2),
        GetLocationForPoint(x2, y1)
    ];

    private static LocationEntity GetLocationForPoint(int x, int y) => Locations.Single(l => l.X == x && l.Y == y);

    public static void AddProduct(ProductEntity product)
    {
        product.Id = ++_productId;
        product.Vendor = Vendors[new Random().Next(Vendors.Count)];
        product.Location = GenerateRandomLocationForProduct(product);
        Products.Add(product);
    }

    public static OrderEntity AddOrder(OrderEntity order)
    {
        order.Id = ++_orderId;
        order.ReferenceId = Guid.NewGuid();
        foreach (var vendorOrder in order.VendorOrders)
        {
            vendorOrder.Id = ++_vendorOrderId;
            vendorOrder.Order = order;
            vendorOrder.OrderId = order.Id;
            foreach (var vendorOrderProduct in vendorOrder.Products)
            {
                vendorOrderProduct.Id = ++_vendorOrderProductId;
                vendorOrderProduct.VendorOrderId = vendorOrder.Id;
                vendorOrderProduct.ProductId = vendorOrderProduct.Product.Id;
            }
        }

        Orders.Add(order);
        return order;
    }
}