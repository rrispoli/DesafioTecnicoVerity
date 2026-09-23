using DailyBalance.Application.Features.Balances.Get;
using FluentAssertions;
using Xunit;

namespace DailyBalance.Application.UnitTests.Features.Balances;

public class GetBalanceQueryValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenValidQuery_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new GetBalanceQueryValidator();
        var query = new GetBalanceQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        // Act
        var validationResult = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenInvalidDate_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new GetBalanceQueryValidator();
        var query = new GetBalanceQuery(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));

        // Act
        var validationResult = await validator.ValidateAsync(query, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.PropertyName == nameof(GetBalanceQuery.Date));
    }
}
