using DailyBalance.Application.Features.Balances.Update;
using DailyBalance.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Contracts.IntegrationEvents.EntryCreated;
using Shared.Domain.Common;
using Xunit;

namespace DailyBalance.Application.UnitTests.Features.Balances;

public class UpdateBalanceCommandHandlerTests
{
    [Theory]
    [InlineData(EntryCreatedType.Credit)]
    [InlineData(EntryCreatedType.Debit)]
    public async Task HandleAsync_WhenValidCommand_ShouldUpdateBalance(EntryCreatedType entryCreatedType)
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var loggerMock = new Mock<ILogger<UpdateBalanceCommandHandler>>();
        var handler = new UpdateBalanceCommandHandler(loggerMock.Object, dbContext);
        var command = new UpdateBalanceCommand(Guid.NewGuid(), entryCreatedType, 100m, DateTimeOffset.UtcNow.AddMinutes(-5));

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        dbContext.Balances.Should().HaveCount(1);
        dbContext.Balances.Should().HaveCount(1);
    }

    [Fact]
    public async Task HandleAsync_WhenDuplicateEvent_ShouldReturnConflictError()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var loggerMock = new Mock<ILogger<UpdateBalanceCommandHandler>>();

        var entryId = Guid.NewGuid();
        var processedEvent = new ProcessedEvent(entryId);
        dbContext.ProcessedEvents.Add(processedEvent);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateBalanceCommandHandler(loggerMock.Object, dbContext);
        var command = new UpdateBalanceCommand(entryId, EntryCreatedType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(-5));

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task HandleAsync_WhenDbUpdateExceptionOccursOnceThenSucceeds_ShouldRetryAndReturnSuccess()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using var dbContext = TestDbContextFactory.CreateThrowing(throwCount: 1, databaseName);
        var loggerMock = new Mock<ILogger<UpdateBalanceCommandHandler>>();
        var handler = new UpdateBalanceCommandHandler(loggerMock.Object, dbContext);
        var command = new UpdateBalanceCommand(Guid.NewGuid(), EntryCreatedType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(-5));

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        dbContext.SaveChangesCallCount.Should().Be(2);

        await using var verificationContext = TestDbContextFactory.Create(databaseName);
        verificationContext.Balances.Should().HaveCount(1);
        verificationContext.ProcessedEvents.Should().HaveCount(1);
        verificationContext.Balances.Single().Credit.Should().Be(100m);
    }

    [Fact]
    public async Task HandleAsync_WhenDbUpdateExceptionPersistsAcrossAllAttempts_ShouldReturnConflictError()
    {
        // Arrange
        var databaseName = Guid.NewGuid().ToString();
        await using var dbContext = TestDbContextFactory.CreateThrowing(throwCount: int.MaxValue, databaseName);
        var loggerMock = new Mock<ILogger<UpdateBalanceCommandHandler>>();
        var handler = new UpdateBalanceCommandHandler(loggerMock.Object, dbContext);
        var command = new UpdateBalanceCommand(Guid.NewGuid(), EntryCreatedType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(-5));

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        dbContext.SaveChangesCallCount.Should().Be(5);

        await using var verificationContext = TestDbContextFactory.Create(databaseName);
        verificationContext.Balances.Should().BeEmpty();
        verificationContext.ProcessedEvents.Should().BeEmpty();
    }
}
