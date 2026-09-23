using DailyBalance.Application.Features.Balances.Get;
using DailyBalance.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace DailyBalance.Application.UnitTests.Features.Balances;

public class GetBalanceQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenFound_ReturnBalance()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new Balance(new DateOnly(2026, 9, 22));
        entity.AddCredit(500);
        entity.AddDebit(300);
        dbContext.Balances.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var expectedResponse = new BalanceResponse(entity);

        var handler = new GetBalanceQueryHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new GetBalanceQuery(entity.Date), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ReturnEmptyBalance()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new Balance(new DateOnly(2026, 9, 22));
        var expectedResponse = new BalanceResponse(entity);

        var handler = new GetBalanceQueryHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new GetBalanceQuery(entity.Date), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(expectedResponse, x => x.Excluding(x => x.UpdatedAt));
    }
}
