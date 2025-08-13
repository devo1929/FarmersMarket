using Core.Classes;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions;

public static class AutoMapperExtensions
{
    public static IServiceCollection AddAutoMapper(this IServiceCollection services) =>
        services.AddAutoMapper(config => config.AddProfile(new DefaultProfile()));
}