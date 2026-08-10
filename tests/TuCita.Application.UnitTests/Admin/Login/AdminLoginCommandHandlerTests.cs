using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Admin.Login;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Options;
using Xunit;

namespace TuCita.Application.UnitTests.Admin.Login;

public class AdminLoginCommandHandlerTests
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAuthTokenGenerator> _authTokenGenerator = new();
    private readonly AdminLoginCommandHandler _handler;

    public AdminLoginCommandHandlerTests()
    {
        var adminOptions = Microsoft.Extensions.Options.Options.Create(new AdminOptions
        {
            Username = "wanderadmin",
            PasswordHash = "hashed-password"
        });

        _handler = new AdminLoginCommandHandler(
            adminOptions,
            _passwordHasher.Object,
            _authTokenGenerator.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        _passwordHasher.Setup(h => h.Verify("wander@admin", "hashed-password")).Returns(true);
        _authTokenGenerator.Setup(g => g.GenerateAdminToken("wanderadmin")).Returns("admin-token");

        // Act
        var response = await _handler.HandleAsync(new AdminLoginCommand("wanderadmin", "wander@admin"));

        // Assert
        response.Token.Should().Be("admin-token");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowUnauthorized_WhenUsernameDoesNotMatch()
    {
        // Act
        var action = () => _handler.HandleAsync(new AdminLoginCommand("otro-usuario", "wander@admin"));

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
        _authTokenGenerator.Verify(g => g.GenerateAdminToken(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowUnauthorized_WhenPasswordIsInvalid()
    {
        // Arrange
        _passwordHasher.Setup(h => h.Verify("wrong-password", "hashed-password")).Returns(false);

        // Act
        var action = () => _handler.HandleAsync(new AdminLoginCommand("wanderadmin", "wrong-password"));

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }
}
