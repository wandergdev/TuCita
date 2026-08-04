using FluentAssertions;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Create_ShouldSucceed_WhenValueHasExactlyTenDigits()
    {
        // Act
        var phone = PhoneNumber.Create("8095551234");

        // Assert
        phone.Value.Should().Be("8095551234");
    }

    [Fact]
    public void Create_ShouldStripFormattingCharacters_WhenValueHasDashesAndSpaces()
    {
        // Act
        var phone = PhoneNumber.Create("(809) 555-1234");

        // Assert
        phone.Value.Should().Be("8095551234");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string value)
    {
        // Act
        Action action = () => PhoneNumber.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("809555123")] // 9 dígitos
    [InlineData("80955512345")] // 11 dígitos
    public void Create_ShouldThrowException_WhenDigitCountIsNotTen(string value)
    {
        // Act
        Action action = () => PhoneNumber.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
