using Entries.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Entries.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<Entry> Entries { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
