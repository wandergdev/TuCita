using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.ScheduleExceptions.CreateScheduleException;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ScheduleExceptions.CreateScheduleException;

public class CreateScheduleExceptionCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IScheduleExceptionRepository> _scheduleExceptionRepository = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly CreateScheduleExceptionCommandHandler _handler;

    private static readonly DateOnly Monday = new(2026, 8, 3);
    private static readonly DateOnly Tuesday = new(2026, 8, 4);

    public CreateScheduleExceptionCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(7, 0)));

        _handler = new CreateScheduleExceptionCommandHandler(
            _professionalRepository.Object,
            _scheduleExceptionRepository.Object,
            _appointmentRepository.Object,
            _identifierGenerator.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateDayOffException_WhenNoConflictingAppointments()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var exceptionId = new ScheduleExceptionId("exception-001");

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _scheduleExceptionRepository.Setup(r => r.GetOverlappingAsync(
                professional.Id, Monday, Tuesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ScheduleException>)[]);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateRangeAsync(
                professional.Id, Monday, Tuesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);
        _identifierGenerator.Setup(g => g.NewScheduleExceptionId()).Returns(exceptionId);

        // Act
        var response = await _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, Monday, Tuesday, null, null, null, null, "Vacaciones"));

        // Assert
        response.ScheduleExceptionId.Should().Be(exceptionId);
        _scheduleExceptionRepository.Verify(r => r.AddAsync(
            It.Is<ScheduleException>(e => e.WorkingHours == null && e.Message == "Vacaciones"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateExceptionWithLunchBreak_WhenSpecialHoursIncludeLunchBreak()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var exceptionId = new ScheduleExceptionId("exception-001");

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _scheduleExceptionRepository.Setup(r => r.GetOverlappingAsync(
                professional.Id, Monday, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ScheduleException>)[]);
        _identifierGenerator.Setup(g => g.NewScheduleExceptionId()).Returns(exceptionId);

        // Act
        var response = await _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id,
            Monday,
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            new TimeOnly(12, 0),
            new TimeOnly(13, 0),
            null));

        // Assert
        response.ScheduleExceptionId.Should().Be(exceptionId);
        _scheduleExceptionRepository.Verify(r => r.AddAsync(
            It.Is<ScheduleException>(e =>
                e.LunchBreak != null
                && e.LunchBreak.StartTime == new TimeOnly(12, 0)
                && e.LunchBreak.EndTime == new TimeOnly(13, 0)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenOnlyOneLunchBreakTimeIsProvided()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, Monday, Monday, new TimeOnly(9, 0), new TimeOnly(17, 0), new TimeOnly(12, 0), null, null));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenLunchBreakProvidedForDayOff()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, Monday, Monday, null, null, new TimeOnly(12, 0), new TimeOnly(13, 0), null));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowConflict_WhenThereAreScheduledAppointmentsInRange()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var conflictingAppointment = Appointment.Create(
            new AppointmentId("appointment-001"),
            professional.Id,
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _scheduleExceptionRepository.Setup(r => r.GetOverlappingAsync(
                professional.Id, Monday, Tuesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ScheduleException>)[]);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateRangeAsync(
                professional.Id, Monday, Tuesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[conflictingAppointment]);

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, Monday, Tuesday, null, null, null, null, null));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowConflict_WhenExceptionOverlapsExistingOne()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var existingException = ScheduleException.Create(
            new ScheduleExceptionId("exception-existing"),
            professional.Id,
            Monday,
            Tuesday,
            workingHours: null,
            message: null);

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _scheduleExceptionRepository.Setup(r => r.GetOverlappingAsync(
                professional.Id, Monday, Tuesday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ScheduleException>)[existingException]);

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, Monday, Tuesday, null, null, null, null, null));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenOnlyOneTimeIsProvided()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, Monday, Monday, new TimeOnly(9, 0), null, null, null, null));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenStartDateIsInThePast()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        var yesterday = Monday.AddDays(-1);

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            professional.Id, yesterday, yesterday, null, null, null, null, null));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        var missingId = new ProfessionalId("professional-missing");
        _professionalRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(new CreateScheduleExceptionCommand(
            missingId, Monday, Monday, null, null, null, null, null));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
