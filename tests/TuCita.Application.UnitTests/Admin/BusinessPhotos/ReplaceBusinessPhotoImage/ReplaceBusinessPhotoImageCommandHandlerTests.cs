using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Admin.BusinessPhotos.ReplaceBusinessPhotoImage;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Admin.BusinessPhotos.ReplaceBusinessPhotoImage;

public class ReplaceBusinessPhotoImageCommandHandlerTests
{
    private readonly Mock<IBusinessPhotoRepository> _businessPhotoRepository = new();
    private readonly Mock<IObjectStorageService> _objectStorageService = new();
    private readonly ReplaceBusinessPhotoImageCommandHandler _handler;

    public ReplaceBusinessPhotoImageCommandHandlerTests()
    {
        _handler = new ReplaceBusinessPhotoImageCommandHandler(
            _businessPhotoRepository.Object,
            _objectStorageService.Object);
    }

    private static BusinessPhoto CreatePhoto() => BusinessPhoto.Create(
        new BusinessPhotoId("photo-001"),
        new BusinessId("business-001"),
        BusinessPhotoKind.Cover,
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "businesses/business-001/original");

    [Fact]
    public async Task HandleAsync_ShouldReplaceImageAndDeleteOldAsset()
    {
        // Arrange
        var photo = CreatePhoto();
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        var command = new ReplaceBusinessPhotoImageCommand(
            photo.Id,
            "https://res.cloudinary.com/demo/image/upload/v2/edited.jpg",
            "businesses/business-001/edited");

        // Act
        await _handler.HandleAsync(command);

        // Assert
        photo.ImageUrl.Should().Be("https://res.cloudinary.com/demo/image/upload/v2/edited.jpg");
        _businessPhotoRepository.Verify(r => r.UpdateAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
        _objectStorageService.Verify(
            s => s.DeleteAsync("businesses/business-001/original", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var missingId = new BusinessPhotoId("photo-missing");
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessPhoto?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new ReplaceBusinessPhotoImageCommand(missingId, "https://example.com/photo.jpg", "public-id"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
