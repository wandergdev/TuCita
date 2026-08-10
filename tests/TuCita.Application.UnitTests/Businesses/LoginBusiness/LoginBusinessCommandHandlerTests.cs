using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.LoginBusiness;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.LoginBusiness;

public class LoginBusinessCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAuthTokenGenerator> _authTokenGenerator = new();
    private readonly LoginBusinessCommandHandler _handler;

    public LoginBusinessCommandHandlerTests()
    {
        _handler = new LoginBusinessCommandHandler(
            _businessRepository.Object,
            _passwordHasher.Object,
            _authTokenGenerator.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var business = BusinessTestData.Create();
        _businessRepository.Setup(r => r.GetByEmailAsync(business.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _passwordHasher.Setup(h => h.Verify("Sup3rSecret!", business.PasswordHash)).Returns(true);
        _authTokenGenerator.Setup(g => g.Generate(business)).Returns("jwt-token");

        // Act
        var response = await _handler.HandleAsync(
            new LoginBusinessCommand(business.Email.Value, "Sup3rSecret!"));

        // Assert
        response.Token.Should().Be("jwt-token");
        response.BusinessId.Should().Be(business.Id);
        response.Slug.Should().Be(business.Slug.Value);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenBusinessDoesNotExist()
    {
        // Arrange
        _businessRepository.Setup(r => r.GetByEmailAsync("missing@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Business?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new LoginBusinessCommand("missing@email.com", "whatever"));

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenEmailIsNotVerified()
    {
        // Arrange
        var business = BusinessTestData.Create(emailVerified: false);
        _businessRepository.Setup(r => r.GetByEmailAsync(business.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _passwordHasher.Setup(h => h.Verify("Sup3rSecret!", business.PasswordHash)).Returns(true);

        // Act
        var action = () => _handler.HandleAsync(
            new LoginBusinessCommand(business.Email.Value, "Sup3rSecret!"));

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>();
    }
}
