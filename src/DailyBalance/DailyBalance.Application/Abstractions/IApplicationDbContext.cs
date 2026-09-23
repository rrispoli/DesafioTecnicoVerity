using DailyBalance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DailyBalance.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Balance> Balances { get; }
    DbSet<ProcessedEvent> ProcessedEvents { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
