using Entries.Application.Features.Entries.Get;
using FluentAssertions;
using Xunit;

namespace Entries.Application.UnitTests.Features.Entries;

public class GetEntryQueryValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenValidQuery_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new GetEntryQueryValidator();
        var query = new GetEntryQuery(Guid.NewGuid());

        // Act
        var validationResult = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenInvalidId_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new GetEntryQueryValidator();
        var query = new GetEntryQuery(Guid.Empty);

        // Act
        var validationResult = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.PropertyName == nameof(GetEntryQuery.Id));
    }
}
