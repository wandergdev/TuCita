using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.GetAvailability;

public class GetAvailabilityQueryHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly GetAvailabilityQueryHandler _handler;

    private static readonly DateOnly Monday = new(2026, 8, 3);

    public GetAvailabilityQueryHandlerTests()
    {
        _handler = new GetAvailabilityQueryHandler(
            _professionalRepository.Object,
            _appointmentRepository.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSlots_WhenProfessionalExists()
    {
        // Arrange: "ahora" son las 7:00, antes de que abra (8:00), así que nada está en el pasado.
        var professional = ProfessionalTestData.Create();
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(7, 0)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, Monday));

        // Assert
        response.AvailableSlots.Should().Contain(new TimeOnly(8, 0));
    }

    [Fact]
    public async Task HandleAsync_ShouldExcludePastSlots_WhenDateIsToday()
    {
        // Arrange: "ahora" son las 9:15, los horarios de 8:00 y 8:30 ya pasaron.
        var professional = ProfessionalTestData.Create();
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(9, 15)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, Monday));

        // Assert
        response.AvailableSlots.Should().NotContain(new TimeOnly(8, 0));
        response.AvailableSlots.Should().NotContain(new TimeOnly(8, 30));
        response.AvailableSlots.Should().Contain(new TimeOnly(9, 30));
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(new GetAvailabilityQuery("missing-slug", Monday));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
