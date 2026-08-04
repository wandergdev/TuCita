using FluentAssertions;
using Moq;
using TuCita.Application.Appointments.CheckExistingAppointment;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.CheckExistingAppointment;

public class CheckExistingAppointmentQueryHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly CheckExistingAppointmentQueryHandler _handler;

    private static readonly DateOnly Monday = new(2026, 8, 3);

    public CheckExistingAppointmentQueryHandlerTests()
    {
        _handler = new CheckExistingAppointmentQueryHandler(
            _professionalRepository.Object,
            _appointmentRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnTrue_WhenCustomerAlreadyHasAScheduledAppointment()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.HasScheduledAppointmentAsync(
                professional.Id, PhoneNumber.Create("8095551234"), Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _handler.HandleAsync(
            new CheckExistingAppointmentQuery(professional.Slug.Value, "8095551234", Monday));

        // Assert
        response.HasExistingAppointment.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFalse_WhenCustomerHasNoScheduledAppointment()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.HasScheduledAppointmentAsync(
                professional.Id, PhoneNumber.Create("8095551234"), Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var response = await _handler.HandleAsync(
            new CheckExistingAppointmentQuery(professional.Slug.Value, "8095551234", Monday));

        // Assert
        response.HasExistingAppointment.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new CheckExistingAppointmentQuery("missing-slug", "8095551234", Monday));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
