using FluentAssertions;
using Moq;
using TuCita.Application.Businesses.RemoveProfessional;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.RemoveProfessional;

public class RemoveProfessionalCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly RemoveProfessionalCommandHandler _handler;
    private static readonly BusinessId OwnerId = new("business-001");

    public RemoveProfessionalCommandHandlerTests()
    {
        _handler = new RemoveProfessionalCommandHandler(_professionalRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldClearBusinessId_WhenRequestedByOwningBusiness()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        professional.JoinBusiness(OwnerId);
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _handler.HandleAsync(new RemoveProfessionalCommand(OwnerId, professional.Id));

        // Assert
        professional.BusinessId.Should().BeNull();
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowForbidden_WhenProfessionalBelongsToAnotherBusiness()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        professional.JoinBusiness(new BusinessId("business-002"));
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(new RemoveProfessionalCommand(OwnerId, professional.Id));

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>();
        _professionalRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Domain.Entities.Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        var missingId = new ProfessionalId("professional-missing");
        _professionalRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(new RemoveProfessionalCommand(OwnerId, missingId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
