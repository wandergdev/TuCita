using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Appointments.GetAppointmentStats;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.GetAppointmentStats;

public class GetAppointmentStatsQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly GetAppointmentStatsQueryHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");

    // Miércoles.
    private static readonly DateTime Now = new(2026, 8, 5, 10, 0, 0);

    public GetAppointmentStatsQueryHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);
        _handler = new GetAppointmentStatsQueryHandler(_appointmentRepository.Object, _dateTimeProvider.Object);
    }

    private void SetupStats(DateOnly expectedFrom, DateOnly expectedTo, AppointmentStatsResult result)
        => _appointmentRepository.Setup(r => r.GetStatsByProfessionalAsync(
                OwnerId, expectedFrom, expectedTo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Fact]
    public async Task HandleAsync_ShouldUseTodayAsRange_WhenRangeIsDay()
    {
        // Arrange
        var today = new DateOnly(2026, 8, 5);
        SetupStats(today, today, new AppointmentStatsResult(3, 1, 2, 3));

        // Act
        var response = await _handler.HandleAsync(new GetAppointmentStatsQuery(OwnerId, StatsRange.Day));

        // Assert
        response.From.Should().Be(today);
        response.To.Should().Be(today);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseMondayToSunday_WhenRangeIsWeek()
    {
        // Arrange: la semana del miércoles 2026-08-05 va del lunes 2026-08-03 al domingo 2026-08-09.
        var monday = new DateOnly(2026, 8, 3);
        var sunday = new DateOnly(2026, 8, 9);
        SetupStats(monday, sunday, new AppointmentStatsResult(3, 1, 2, 3));

        // Act
        var response = await _handler.HandleAsync(new GetAppointmentStatsQuery(OwnerId, StatsRange.Week));

        // Assert
        response.From.Should().Be(monday);
        response.To.Should().Be(sunday);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseFirstToLastDayOfMonth_WhenRangeIsMonth()
    {
        // Arrange
        var firstDay = new DateOnly(2026, 8, 1);
        var lastDay = new DateOnly(2026, 8, 31);
        SetupStats(firstDay, lastDay, new AppointmentStatsResult(3, 1, 2, 3));

        // Act
        var response = await _handler.HandleAsync(new GetAppointmentStatsQuery(OwnerId, StatsRange.Month));

        // Assert
        response.From.Should().Be(firstDay);
        response.To.Should().Be(lastDay);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapStatsResultToResponse()
    {
        // Arrange
        var today = new DateOnly(2026, 8, 5);
        SetupStats(today, today, new AppointmentStatsResult(
            CompletedCount: 12,
            NoShowCount: 2,
            CancelledCount: 4,
            DistinctCustomersCompleted: 10));

        // Act
        var response = await _handler.HandleAsync(new GetAppointmentStatsQuery(OwnerId, StatsRange.Day));

        // Assert
        response.CompletedCount.Should().Be(12);
        response.NoShowCount.Should().Be(2);
        response.CancelledCount.Should().Be(4);
        response.DistinctCustomersCount.Should().Be(10);
    }
}
