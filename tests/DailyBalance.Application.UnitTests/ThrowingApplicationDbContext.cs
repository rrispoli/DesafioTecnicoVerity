using DailyBalance.Application.Abstractions;
using DailyBalance.Domain.Entities;
using DailyBalance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyBalance.Application.UnitTests;

internal sealed class ThrowingApplicationDbContext(DbContextOptions options, int throwCount)
    : DbContext(options), IApplicationDbContext
{
    public int SaveChangesCallCount { get; private set; }

    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        if (SaveChangesCallCount <= throwCount)
        {
            throw new DbUpdateException("Simulated unique constraint violation.", new Exception("Duplicate key"));
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
