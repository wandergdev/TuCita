using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.Entities;

public class ScheduleExceptionTests
{
    private static readonly DateOnly Monday = new(2026, 8, 3);
    private static readonly DateOnly Tuesday = new(2026, 8, 4);
    private static readonly ProfessionalId ProfessionalId = new("professional-001");

    [Fact]
    public void Create_ShouldCreateDayOffException_WhenWorkingHoursIsNull()
    {
        // Act
        var exception = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Tuesday,
            workingHours: null,
            message: "  Vacaciones  ");

        // Assert
        exception.StartDate.Should().Be(Monday);
        exception.EndDate.Should().Be(Tuesday);
        exception.WorkingHours.Should().BeNull();
        exception.Message.Should().Be("Vacaciones");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenEndDateIsBeforeStartDate()
    {
        // Act
        Action action = () => ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Tuesday,
            Monday,
            workingHours: null,
            message: null);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldStoreNullMessage_WhenMessageIsEmptyOrWhitespace()
    {
        // Act
        var exception = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Monday,
            workingHours: null,
            message: "   ");

        // Assert
        exception.Message.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenMessageExceedsMaxLength()
    {
        // Arrange
        var tooLongMessage = new string('a', 301);

        // Act
        Action action = () => ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Monday,
            workingHours: null,
            message: tooLongMessage);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldAcceptLunchBreak_WhenWithinSpecialWorkingHours()
    {
        // Act
        var exception = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Monday,
            workingHours: TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0)),
            lunchBreak: TimeRange.Create(new TimeOnly(12, 0), new TimeOnly(13, 0)),
            message: null);

        // Assert
        exception.LunchBreak.Should().NotBeNull();
        exception.LunchBreak!.StartTime.Should().Be(new TimeOnly(12, 0));
    }

    [Fact]
    public void Create_ShouldThrowException_WhenLunchBreakIsOutsideSpecialWorkingHours()
    {
        // Act
        Action action = () => ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Monday,
            workingHours: TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0)),
            lunchBreak: TimeRange.Create(new TimeOnly(17, 0), new TimeOnly(18, 0)),
            message: null);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenLunchBreakProvidedForDayOff()
    {
        // Act
        Action action = () => ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Monday,
            workingHours: null,
            lunchBreak: TimeRange.Create(new TimeOnly(12, 0), new TimeOnly(13, 0)),
            message: null);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CoversDate_ShouldReturnTrue_WhenDateIsWithinRange()
    {
        // Arrange
        var exception = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Tuesday,
            workingHours: null,
            message: null);

        // Act & Assert
        exception.CoversDate(Monday).Should().BeTrue();
        exception.CoversDate(Tuesday).Should().BeTrue();
        exception.CoversDate(Monday.AddDays(-1)).Should().BeFalse();
        exception.CoversDate(Tuesday.AddDays(1)).Should().BeFalse();
    }

    [Fact]
    public void OverlapsWith_ShouldReturnTrue_WhenRangesIntersect()
    {
        // Arrange
        var exception = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            ProfessionalId,
            Monday,
            Tuesday,
            workingHours: null,
            message: null);

        // Act & Assert
        exception.OverlapsWith(Tuesday, Tuesday.AddDays(1)).Should().BeTrue();
        exception.OverlapsWith(Tuesday.AddDays(1), Tuesday.AddDays(2)).Should().BeFalse();
    }
}
