using Entries.Application.Abstractions;
using Entries.Infrastructure.Messaging;
using Entries.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Entries.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IEventPublisher, EventPublisher>();
    }
}
