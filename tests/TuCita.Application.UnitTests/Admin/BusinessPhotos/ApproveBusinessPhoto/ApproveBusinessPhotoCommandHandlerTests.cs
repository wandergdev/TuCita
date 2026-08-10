using FluentAssertions;
using Moq;
using TuCita.Application.Admin.BusinessPhotos.ApproveBusinessPhoto;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Admin.BusinessPhotos.ApproveBusinessPhoto;

public class ApproveBusinessPhotoCommandHandlerTests
{
    private readonly Mock<IBusinessPhotoRepository> _businessPhotoRepository = new();
    private readonly ApproveBusinessPhotoCommandHandler _handler;

    public ApproveBusinessPhotoCommandHandlerTests()
    {
        _handler = new ApproveBusinessPhotoCommandHandler(_businessPhotoRepository.Object);
    }

    private static BusinessPhoto CreatePhoto() => BusinessPhoto.Create(
        new BusinessPhotoId("photo-001"),
        new BusinessId("business-001"),
        BusinessPhotoKind.Cover,
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "businesses/business-001/photo");

    [Fact]
    public async Task HandleAsync_ShouldApprovePhoto_WhenPending()
    {
        // Arrange
        var photo = CreatePhoto();
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(photo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(photo);

        // Act
        await _handler.HandleAsync(new ApproveBusinessPhotoCommand(photo.Id));

        // Assert
        photo.Status.Should().Be(PortfolioPhotoStatus.Approved);
        _businessPhotoRepository.Verify(r => r.UpdateAsync(photo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenPhotoDoesNotExist()
    {
        // Arrange
        var missingId = new BusinessPhotoId("photo-missing");
        _businessPhotoRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessPhoto?)null);

        // Act
        var action = () => _handler.HandleAsync(new ApproveBusinessPhotoCommand(missingId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
