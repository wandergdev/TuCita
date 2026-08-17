using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Professionals.JoinBusiness;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.JoinBusiness;

public class JoinBusinessCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IBusinessInviteRepository> _businessInviteRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly JoinBusinessCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 16, 8, 0, 0);
    private static readonly BusinessId TargetBusinessId = new("business-001");

    public JoinBusinessCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        _handler = new JoinBusinessCommandHandler(
            _professionalRepository.Object,
            _businessRepository.Object,
            _businessInviteRepository.Object,
            _dateTimeProvider.Object);
    }

    private static BusinessInvite CreateInvite(DateTime? expiresAt = null) => BusinessInvite.Create(
        new BusinessInviteId("invite-001"),
        TargetBusinessId,
        "valid-token",
        expiresAt ?? Now.AddDays(1));

    [Fact]
    public async Task HandleAsync_ShouldJoinBusinessAndConsumeInvite_WhenInviteIsValid()
    {
        // Arrange
        var invite = CreateInvite();
        var professional = ProfessionalTestData.Create();
        var business = BusinessTestData.Create(id: TargetBusinessId.Value);

        _businessInviteRepository.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _businessRepository.Setup(r => r.GetByIdAsync(TargetBusinessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        var response = await _handler.HandleAsync(new JoinBusinessCommand(professional.Id, "valid-token"));

        // Assert
        response.BusinessName.Should().Be(business.Name);
        professional.BusinessId.Should().Be(TargetBusinessId);
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
        _businessInviteRepository.Verify(r => r.UpdateAsync(
            It.Is<BusinessInvite>(i => i.UsedAt != null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenInviteDoesNotExist()
    {
        // Arrange
        _businessInviteRepository.Setup(r => r.GetByTokenAsync("bad-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessInvite?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new JoinBusinessCommand(new ProfessionalId("professional-001"), "bad-token"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenInviteExpired()
    {
        // Arrange
        var invite = CreateInvite(expiresAt: Now.AddDays(-1));
        _businessInviteRepository.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        // Act
        var action = () => _handler.HandleAsync(
            new JoinBusinessCommand(new ProfessionalId("professional-001"), "valid-token"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperationException_WhenProfessionalAlreadyBelongsToABusiness()
    {
        // Arrange
        var invite = CreateInvite();
        var professional = ProfessionalTestData.Create();
        professional.JoinBusiness(new BusinessId("another-business"));

        _businessInviteRepository.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(new JoinBusinessCommand(professional.Id, "valid-token"));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
        _businessInviteRepository.Verify(
            r => r.UpdateAsync(It.IsAny<BusinessInvite>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
