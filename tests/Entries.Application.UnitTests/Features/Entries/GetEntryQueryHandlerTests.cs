using Entries.Application.Features.Entries.Get;
using Entries.Domain.Entities;
using Entries.Domain.Enums;
using FluentAssertions;
using Shared.Domain.Common;
using Xunit;

namespace Entries.Application.UnitTests.Features.Entries;

public class GetEntryQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenFound_ReturnEntry()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var entity = new Entry(EntryType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(-5), "Test entry");
        dbContext.Entries.Add(entity);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var expectedResponse = new EntryResponse(entity);

        var handler = new GetEntryQueryHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new GetEntryQuery(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task HandleAsync_WhenNotFound_ReturnNotFoundError()
    {
        // Arrange
        await using var dbContext = TestDbContextFactory.Create();
        var handler = new GetEntryQueryHandler(dbContext);

        // Act
        var result = await handler.HandleAsync(new GetEntryQuery(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
