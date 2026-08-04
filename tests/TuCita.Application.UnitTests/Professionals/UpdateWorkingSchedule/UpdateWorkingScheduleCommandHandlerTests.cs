using FluentAssertions;
using Moq;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.UpdateWorkingSchedule;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.UpdateWorkingSchedule;

public class UpdateWorkingScheduleCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly UpdateWorkingScheduleCommandHandler _handler;

    public UpdateWorkingScheduleCommandHandlerTests()
    {
        _handler = new UpdateWorkingScheduleCommandHandler(_professionalRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReplaceWorkingSchedule_WhenProfessionalExists()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var newSchedule = new WorkingSchedule(
        [
            new ScheduleRule(
                new WorkDays([DayOfWeek.Saturday]),
                TimeRange.Create(new TimeOnly(10, 0), new TimeOnly(14, 0)))
        ]);

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _handler.HandleAsync(new UpdateWorkingScheduleCommand(professional.Id, newSchedule));

        // Assert
        professional.WorkingSchedule.Should().BeSameAs(newSchedule);
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        var missingId = new ProfessionalId("professional-missing");
        _professionalRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new UpdateWorkingScheduleCommand(missingId, WorkingScheduleTestData.Create()));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
