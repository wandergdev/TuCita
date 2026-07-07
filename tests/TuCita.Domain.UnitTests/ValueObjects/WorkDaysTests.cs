using FluentAssertions;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.ValueObjects;

public class WorkDaysTests
{
    [Fact]
    public void Constructor_ShouldThrowException_WhenNoWorkingDaysAreProvided()
    {
        //Arrange
        var days = Enumerable.Empty<DayOfWeek>();
        
        //Act
        Action action = () => new WorkDays(days);
        
        //Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*al menos un día laboral*");
    }

    [Fact]
    public void Contains_ShouldReturnTrue_WhenDaysExist()
    {
        //Arrange
        var workDays = new WorkDays(new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday
        });
        
        //Act
        var result = workDays.Contains(DayOfWeek.Monday);
        
        //Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void Contains_ShouldReturnTrue_WhenDaysDoesNotExist()
    {
        //Arrange
        var workDays = new WorkDays(new[]
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday
        });
        
        //Act
        var result = workDays.Contains(DayOfWeek.Friday);
        
        //Assert
        result.Should().BeFalse();
    }
}