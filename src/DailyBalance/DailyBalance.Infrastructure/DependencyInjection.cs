using DailyBalance.Application.Abstractions;
using DailyBalance.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DailyBalance.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
    }
}
