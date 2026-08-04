using FluentAssertions;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.ValueObjects;

public class ScheduleRuleTests
{
    private static readonly WorkDays Weekdays = new([DayOfWeek.Monday]);
    private static readonly TimeRange WorkingHours = TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(17, 0));

    [Fact]
    public void Constructor_ShouldAllowNullLunchBreak()
    {
        // Act
        var rule = new ScheduleRule(Weekdays, WorkingHours);

        // Assert
        rule.LunchBreak.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldAcceptLunchBreak_WhenWithinWorkingHours()
    {
        // Arrange
        var lunchBreak = TimeRange.Create(new TimeOnly(12, 0), new TimeOnly(13, 0));

        // Act
        var rule = new ScheduleRule(Weekdays, WorkingHours, lunchBreak);

        // Assert
        rule.LunchBreak.Should().Be(lunchBreak);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenLunchBreakStartsBeforeWorkingHours()
    {
        // Arrange
        var lunchBreak = TimeRange.Create(new TimeOnly(8, 0), new TimeOnly(9, 30));

        // Act
        Action action = () => new ScheduleRule(Weekdays, WorkingHours, lunchBreak);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenLunchBreakEndsAfterWorkingHours()
    {
        // Arrange
        var lunchBreak = TimeRange.Create(new TimeOnly(16, 30), new TimeOnly(17, 30));

        // Act
        Action action = () => new ScheduleRule(Weekdays, WorkingHours, lunchBreak);

        // Assert
        action.Should().Throw<ArgumentException>();
    }
}
