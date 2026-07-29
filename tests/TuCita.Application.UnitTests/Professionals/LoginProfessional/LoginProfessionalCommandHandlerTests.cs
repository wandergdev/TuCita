using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.LoginProfessional;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.LoginProfessional;

public class LoginProfessionalCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IAuthTokenGenerator> _authTokenGenerator = new();
    private readonly LoginProfessionalCommandHandler _handler;

    public LoginProfessionalCommandHandlerTests()
    {
        _handler = new LoginProfessionalCommandHandler(
            _professionalRepository.Object,
            _passwordHasher.Object,
            _authTokenGenerator.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetByEmailAsync(professional.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _passwordHasher.Setup(h => h.Verify("Sup3rSecret!", professional.PasswordHash)).Returns(true);
        _authTokenGenerator.Setup(g => g.Generate(professional)).Returns("jwt-token");

        // Act
        var response = await _handler.HandleAsync(
            new LoginProfessionalCommand(professional.Email.Value, "Sup3rSecret!"));

        // Assert
        response.Token.Should().Be("jwt-token");
        response.ProfessionalId.Should().Be(professional.Id);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenProfessionalDoesNotExist()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetByEmailAsync("missing@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new LoginProfessionalCommand("missing@email.com", "whatever"));

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenPasswordIsInvalid()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetByEmailAsync(professional.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _passwordHasher.Setup(h => h.Verify("wrong-password", professional.PasswordHash)).Returns(false);

        // Act
        var action = () => _handler.HandleAsync(
            new LoginProfessionalCommand(professional.Email.Value, "wrong-password"));

        // Assert
        await action.Should().ThrowAsync<UnauthorizedException>();
    }
}
