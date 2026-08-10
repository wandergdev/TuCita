using FluentAssertions;
using Moq;
using TuCita.Application.Businesses.GetMyRoster;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.GetMyRoster;

public class GetMyRosterQueryHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly GetMyRosterQueryHandler _handler;
    private static readonly BusinessId OwnerId = new("business-001");

    public GetMyRosterQueryHandlerTests()
    {
        _handler = new GetMyRosterQueryHandler(_professionalRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnProfessionalsOfThatBusiness()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        professional.JoinBusiness(OwnerId);
        _professionalRepository.Setup(r => r.GetByBusinessIdAsync(OwnerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Professional>)[professional]);

        // Act
        var response = await _handler.HandleAsync(new GetMyRosterQuery(OwnerId));

        // Assert
        response.Professionals.Should().ContainSingle();
        response.Professionals.First().FullName.Should().Be(professional.FullName);
        response.Professionals.First().Slug.Should().Be(professional.Slug.Value);
    }
}
