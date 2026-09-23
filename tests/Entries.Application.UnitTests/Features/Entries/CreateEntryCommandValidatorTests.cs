using Entries.Application.Features.Entries.Create;
using Entries.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace Entries.Application.UnitTests.Features.Entries;

public class CreateEntryCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var validator = new CreateEntryCommandValidator();
        var command = new CreateEntryCommand(EntryType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(-5), "Test entry");

        // Act
        var validationResult = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenInvalidType_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateEntryCommandValidator();
        var command = new CreateEntryCommand((EntryType)999, 100m, DateTimeOffset.UtcNow.AddMinutes(-5), "Test entry");
        
        // Act
        var validationResult = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.PropertyName == nameof(CreateEntryCommand.Type));
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    public async Task ValidateAsync_WhenInvalidAmount_ShouldHaveValidationError(decimal amount)
    {
        // Arrange
        var validator = new CreateEntryCommandValidator();
        var command = new CreateEntryCommand(EntryType.Credit, amount, DateTimeOffset.UtcNow.AddMinutes(-5), "Test entry");

        // Act
        var validationResult = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.PropertyName == nameof(CreateEntryCommand.Amount));
    }

    [Fact]
    public async Task ValidateAsync_WhenInvalidOccurredAt_ShouldHaveValidationError()
    {
        // Arrange
        var validator = new CreateEntryCommandValidator();
        var command = new CreateEntryCommand(EntryType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(5), "Test entry");

        // Act
        var validationResult = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.PropertyName == nameof(CreateEntryCommand.OccurredAt));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus")]
    public async Task ValidateAsync_WhenInvalidDescription_ShouldHaveValidationError(string description)
    {
        // Arrange
        var validator = new CreateEntryCommandValidator();
        var command = new CreateEntryCommand(EntryType.Credit, 100m, DateTimeOffset.UtcNow.AddMinutes(-5), description);
        // Act
        var validationResult = await validator.ValidateAsync(command, TestContext.Current.CancellationToken);
        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(x => x.PropertyName == nameof(CreateEntryCommand.Description));
    }
}
