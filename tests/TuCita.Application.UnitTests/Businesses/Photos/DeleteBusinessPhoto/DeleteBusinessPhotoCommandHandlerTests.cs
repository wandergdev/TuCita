using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.Photos.DeleteBusinessPhoto;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.Photos.DeleteBusinessPhoto;

public class DeleteBusinessPhotoCommandHandlerTests
{
    private readonly Mock<IBusinessPhotoRepository> _businessPhotoRepository = new();
    private readonly Mock<IObjectStorageService> _objectStorageService = new();
    private readonly DeleteBusinessPhotoCommandHandler _handler;
    private static readonly BusinessId OwnerId = new("business-001");

    public DeleteBusinessPhotoCommandHandlerTests()
    {
        _handler = new DeleteBusinessPhotoCommandHandler(
            _businessPhotoRepository.Object,
            _objectStorageService.Object);
    }

    private static BusinessPhoto CreatePhoto() => BusinessPhoto.Create(
        new BusinessPhotoId("photo-001"),
        OwnerId,
        BusinessPhotoKind.Space,
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "businesses/business-001/photo");

    [Fact]
    public async Task HandleAsync_ShouldDeletePhotoFromStorageAndRepository_WhenRequestedByOwner()
    {
        // Arrange
        var photo = CreatePhoto();
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        await _handler.HandleAsync(new DeleteBusinessPhotoCommand(photo.Id, OwnerId));

        // Assert
        _objectStorageService.Verify(
            s => s.DeleteAsync(photo.PublicId, It.IsAny<CancellationToken>()), Times.Once);
        _businessPhotoRepository.Verify(
            r => r.DeleteAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowForbidden_WhenRequestedByAnotherBusiness()
    {
        // Arrange
        var photo = CreatePhoto();
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);
        var anotherBusinessId = new BusinessId("business-002");

        // Act
        var action = () => _handler.HandleAsync(new DeleteBusinessPhotoCommand(photo.Id, anotherBusinessId));

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>();
        _objectStorageService.Verify(
            s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var missingId = new BusinessPhotoId("photo-missing");
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessPhoto?)null);

        // Act
        var action = () => _handler.HandleAsync(new DeleteBusinessPhotoCommand(missingId, OwnerId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
