using FluentAssertions;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.ValueObjects;

public class SlugTests
{
    [Theory]
    [InlineData("jose-barber")]
    [InlineData("jose-barber-2")]
    [InlineData("josebarber")]
    public void Create_ShouldSucceed_WhenValueIsAValidSlug(string value)
    {
        // Act
        var slug = Slug.Create(value);

        // Assert
        slug.Value.Should().Be(value);
    }

    [Fact]
    public void Create_ShouldLowercaseValue_WhenValueHasUppercaseLetters()
    {
        // Act
        var slug = Slug.Create("Jose-Barber");

        // Assert
        slug.Value.Should().Be("jose-barber");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenValueIsEmpty(string value)
    {
        // Act
        Action action = () => Slug.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("josé-barber")] // tilde
    [InlineData("jose barber")] // espacio
    [InlineData("jose_barber")] // guion bajo
    [InlineData("-jose-barber")] // empieza con guion
    [InlineData("jose-barber-")] // termina con guion
    [InlineData("jose--barber")] // guiones repetidos
    [InlineData("josé")]
    public void Create_ShouldThrowException_WhenValueHasInvalidCharacters(string value)
    {
        // Act
        Action action = () => Slug.Create(value);

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*sin tildes*");
    }
}
