using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.Portfolio.DeletePortfolioPhoto;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.Portfolio.DeletePortfolioPhoto;

public class DeletePortfolioPhotoCommandHandlerTests
{
    private readonly Mock<IPortfolioPhotoRepository> _portfolioPhotoRepository = new();
    private readonly Mock<IObjectStorageService> _objectStorageService = new();
    private readonly DeletePortfolioPhotoCommandHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");

    public DeletePortfolioPhotoCommandHandlerTests()
    {
        _handler = new DeletePortfolioPhotoCommandHandler(
            _portfolioPhotoRepository.Object,
            _objectStorageService.Object);
    }

    private static PortfolioPhoto CreatePhoto() => PortfolioPhoto.Create(
        new PortfolioPhotoId("photo-001"),
        OwnerId,
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "professionals/professional-001/photo");

    [Fact]
    public async Task HandleAsync_ShouldDeletePhotoFromStorageAndRepository_WhenRequestedByOwner()
    {
        // Arrange
        var photo = CreatePhoto();
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        await _handler.HandleAsync(new DeletePortfolioPhotoCommand(photo.Id, OwnerId));

        // Assert
        _objectStorageService.Verify(
            s => s.DeleteAsync(photo.PublicId, It.IsAny<CancellationToken>()), Times.Once);
        _portfolioPhotoRepository.Verify(
            r => r.DeleteAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowForbidden_WhenRequestedByAnotherProfessional()
    {
        // Arrange
        var photo = CreatePhoto();
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);
        var anotherProfessionalId = new ProfessionalId("professional-002");

        // Act
        var action = () => _handler.HandleAsync(new DeletePortfolioPhotoCommand(photo.Id, anotherProfessionalId));

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>();
        _objectStorageService.Verify(
            s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var missingId = new PortfolioPhotoId("photo-missing");
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioPhoto?)null);

        // Act
        var action = () => _handler.HandleAsync(new DeletePortfolioPhotoCommand(missingId, OwnerId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
