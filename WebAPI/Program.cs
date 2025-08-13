using Core.Extensions;
using WebAPI.Database;
using WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers();

builder.Services
    .AddAutoMapper();

builder.Services
    .AddScoped<VendorRepository>()
    .AddScoped<ProductsRepository>()
    .AddScoped<OrderRepository>()
    .AddScoped<UserRepository>();

builder.Services
    .AddScoped<VendorService>()
    .AddScoped<ProductService>()
    .AddScoped<OrderService>()
    .AddScoped<AuthenticateService>()
    .AddScoped<TokenService>();

var app = builder.Build();
app.UseHttpsRedirection()
    .UseRouting()
    .UseAuthorization();

app.MapControllers();

app.Run();