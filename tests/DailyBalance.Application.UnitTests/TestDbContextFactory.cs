using DailyBalance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyBalance.Application.UnitTests;

internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static ThrowingApplicationDbContext CreateThrowing(int throwCount, string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new ThrowingApplicationDbContext(options, throwCount);
    }
}
