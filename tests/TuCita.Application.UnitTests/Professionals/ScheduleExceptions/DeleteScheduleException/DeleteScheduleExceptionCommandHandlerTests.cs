using FluentAssertions;
using Moq;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.ScheduleExceptions.DeleteScheduleException;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ScheduleExceptions.DeleteScheduleException;

public class DeleteScheduleExceptionCommandHandlerTests
{
    private readonly Mock<IScheduleExceptionRepository> _scheduleExceptionRepository = new();
    private readonly DeleteScheduleExceptionCommandHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");

    public DeleteScheduleExceptionCommandHandlerTests()
    {
        _handler = new DeleteScheduleExceptionCommandHandler(_scheduleExceptionRepository.Object);
    }

    private static ScheduleException CreateException() => ScheduleException.Create(
        new ScheduleExceptionId("exception-001"),
        OwnerId,
        new DateOnly(2026, 8, 3),
        new DateOnly(2026, 8, 3),
        workingHours: null,
        message: null);

    [Fact]
    public async Task HandleAsync_ShouldDeleteException_WhenRequestedByOwner()
    {
        // Arrange
        var exception = CreateException();
        _scheduleExceptionRepository.Setup(r => r.GetByIdAsync(exception.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exception);

        // Act
        await _handler.HandleAsync(new DeleteScheduleExceptionCommand(exception.Id, OwnerId));

        // Assert
        _scheduleExceptionRepository.Verify(r => r.DeleteAsync(exception, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowForbidden_WhenRequestedByAnotherProfessional()
    {
        // Arrange
        var exception = CreateException();
        _scheduleExceptionRepository.Setup(r => r.GetByIdAsync(exception.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exception);
        var anotherProfessionalId = new ProfessionalId("professional-002");

        // Act
        var action = () => _handler.HandleAsync(
            new DeleteScheduleExceptionCommand(exception.Id, anotherProfessionalId));

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>();
        _scheduleExceptionRepository.Verify(
            r => r.DeleteAsync(It.IsAny<ScheduleException>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenExceptionDoesNotExist()
    {
        // Arrange
        var missingId = new ScheduleExceptionId("exception-missing");
        _scheduleExceptionRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ScheduleException?)null);

        // Act
        var action = () => _handler.HandleAsync(new DeleteScheduleExceptionCommand(missingId, OwnerId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
