using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ScheduleExceptions.GetScheduleExceptions;

public class GetScheduleExceptionsQueryHandlerTests
{
    private readonly Mock<IScheduleExceptionRepository> _scheduleExceptionRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly GetScheduleExceptionsQueryHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");
    private static readonly DateOnly Monday = new(2026, 8, 3);

    public GetScheduleExceptionsQueryHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(7, 0)));
        _handler = new GetScheduleExceptionsQueryHandler(
            _scheduleExceptionRepository.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSummaries_MappedFromExceptions()
    {
        // Arrange
        var dayOff = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            OwnerId,
            Monday,
            Monday.AddDays(1),
            workingHours: null,
            message: "Vacaciones");
        var specialHours = ScheduleException.Create(
            new ScheduleExceptionId("exception-002"),
            OwnerId,
            Monday.AddDays(5),
            Monday.AddDays(5),
            workingHours: TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(12, 0)),
            message: null);

        _scheduleExceptionRepository.Setup(r => r.GetByProfessionalIdAsync(
                OwnerId, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<ScheduleException>)[specialHours, dayOff]);

        // Act
        var response = await _handler.HandleAsync(new GetScheduleExceptionsQuery(OwnerId));

        // Assert: ordenadas por fecha de inicio.
        response.Exceptions.Should().HaveCount(2);
        response.Exceptions.First().Id.Should().Be(dayOff.Id);
        response.Exceptions.First().Message.Should().Be("Vacaciones");
        response.Exceptions.First().StartTime.Should().BeNull();
        response.Exceptions.Last().Id.Should().Be(specialHours.Id);
        response.Exceptions.Last().StartTime.Should().Be(new TimeOnly(10, 0));
        response.Exceptions.Last().EndTime.Should().Be(new TimeOnly(12, 0));
    }
}
