using Entries.Application.Features.Entries.Create;
using Entries.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Entries.Application.UnitTests.Features.Entries;

public class CreateEntryCommandHandlerTests
{
    [Theory]
    [InlineData(EntryType.Credit)]
    [InlineData(EntryType.Debit)]
    public async Task HandleAsync_WhenValidCommand_ShouldCreateEntryAndReturnId(EntryType entryType)
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new CreateEntryCommandHandler(dbContext);
        var command = new CreateEntryCommand(entryType, 100m, DateTimeOffset.UtcNow.AddMinutes(-5), "Test entry");

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        dbContext.Entries.Should().HaveCount(1);
        dbContext.OutboxMessages.Should().HaveCount(1);
    }
}
