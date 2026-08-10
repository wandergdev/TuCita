using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Admin.Portfolio.ReplacePortfolioPhotoImage;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Admin.Portfolio.ReplacePortfolioPhotoImage;

public class ReplacePortfolioPhotoImageCommandHandlerTests
{
    private readonly Mock<IPortfolioPhotoRepository> _portfolioPhotoRepository = new();
    private readonly Mock<IObjectStorageService> _objectStorageService = new();
    private readonly ReplacePortfolioPhotoImageCommandHandler _handler;

    public ReplacePortfolioPhotoImageCommandHandlerTests()
    {
        _handler = new ReplacePortfolioPhotoImageCommandHandler(
            _portfolioPhotoRepository.Object,
            _objectStorageService.Object);
    }

    private static PortfolioPhoto CreatePhoto() => PortfolioPhoto.Create(
        new PortfolioPhotoId("photo-001"),
        new ProfessionalId("professional-001"),
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "professionals/professional-001/original");

    [Fact]
    public async Task HandleAsync_ShouldReplaceImageAndDeleteOldAsset()
    {
        // Arrange
        var photo = CreatePhoto();
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var command = new ReplacePortfolioPhotoImageCommand(
            photo.Id,
            "https://res.cloudinary.com/demo/image/upload/v2/edited.jpg",
            "professionals/professional-001/edited");

        // Act
        await _handler.HandleAsync(command);

        // Assert
        photo.ImageUrl.Should().Be("https://res.cloudinary.com/demo/image/upload/v2/edited.jpg");
        photo.PublicId.Should().Be("professionals/professional-001/edited");
        _portfolioPhotoRepository.Verify(r => r.UpdateAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
        _objectStorageService.Verify(
            s => s.DeleteAsync("professionals/professional-001/original", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var missingId = new PortfolioPhotoId("photo-missing");
        _portfolioPhotoRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PortfolioPhoto?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new ReplacePortfolioPhotoImageCommand(missingId, "https://example.com/photo.jpg", "public-id"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
