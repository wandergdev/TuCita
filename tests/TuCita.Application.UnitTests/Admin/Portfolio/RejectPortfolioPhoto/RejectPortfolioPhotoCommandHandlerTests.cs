using FluentAssertions;
using Moq;
using TuCita.Application.Admin.Portfolio.RejectPortfolioPhoto;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Admin.Portfolio.RejectPortfolioPhoto;

public class RejectPortfolioPhotoCommandHandlerTests
{
    private readonly Mock<IPortfolioPhotoRepository> _portfolioPhotoRepository = new();
    private readonly RejectPortfolioPhotoCommandHandler _handler;

    public RejectPortfolioPhotoCommandHandlerTests()
    {
        _handler = new RejectPortfolioPhotoCommandHandler(_portfolioPhotoRepository.Object);
    }

    private static PortfolioPhoto CreatePhoto() => PortfolioPhoto.Create(
        new PortfolioPhotoId("photo-001"),
        new ProfessionalId("professional-001"),
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "professionals/professional-001/photo");

    [Fact]
    public async Task HandleAsync_ShouldRejectPhoto_WhenPending()
    {
        // Arrange
        var photo = CreatePhoto();
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        await _handler.HandleAsync(new RejectPortfolioPhotoCommand(photo.Id));

        // Assert
        photo.Status.Should().Be(PortfolioPhotoStatus.Rejected);
        _portfolioPhotoRepository.Verify(r => r.UpdateAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenPhotoIsNotPending()
    {
        // Arrange
        var photo = CreatePhoto();
        photo.Approve();
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        var action = () => _handler.HandleAsync(new RejectPortfolioPhotoCommand(photo.Id));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var missingId = new PortfolioPhotoId("photo-missing");
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioPhoto?)null);

        // Act
        var action = () => _handler.HandleAsync(new RejectPortfolioPhotoCommand(missingId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
