using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.Photos.AddBusinessPhoto;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.Photos.AddBusinessPhoto;

public class AddBusinessPhotoCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IBusinessPhotoRepository> _businessPhotoRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly AddBusinessPhotoCommandHandler _handler;

    public AddBusinessPhotoCommandHandlerTests()
    {
        _handler = new AddBusinessPhotoCommandHandler(
            _businessRepository.Object,
            _businessPhotoRepository.Object,
            _identifierGenerator.Object);
    }

    private static AddBusinessPhotoCommand CreateCommand(BusinessId businessId, BusinessPhotoKind kind) => new(
        businessId,
        kind,
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "businesses/business-001/photo",
        null);

    private static BusinessPhoto CreateExistingPhoto(BusinessId businessId, BusinessPhotoKind kind, int index)
        => BusinessPhoto.Create(
            new BusinessPhotoId($"photo-{index}"),
            businessId,
            kind,
            "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
            $"businesses/business-001/photo-{index}");

    [Fact]
    public async Task HandleAsync_ShouldAddPhoto_WhenUnderTheLimit()
    {
        // Arrange
        var business = BusinessTestData.Create();
        var photoId = new BusinessPhotoId("photo-001");

        _businessRepository.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _businessPhotoRepository
            .Setup(r => r.GetByBusinessIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)[]);
        _identifierGenerator.Setup(g => g.NewBusinessPhotoId()).Returns(photoId);

        // Act
        var response = await _handler.HandleAsync(CreateCommand(business.Id, BusinessPhotoKind.Cover));

        // Assert
        response.BusinessPhotoId.Should().Be(photoId);
        _businessPhotoRepository.Verify(
            r => r.AddAsync(It.IsAny<BusinessPhoto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAtTheLimitForCover()
    {
        // Arrange
        var business = BusinessTestData.Create();
        var existingPhotos = new[] { CreateExistingPhoto(business.Id, BusinessPhotoKind.Cover, 1) };

        _businessRepository.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _businessPhotoRepository
            .Setup(r => r.GetByBusinessIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)existingPhotos);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(business.Id, BusinessPhotoKind.Cover));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
        _businessPhotoRepository.Verify(
            r => r.AddAsync(It.IsAny<BusinessPhoto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAtTheLimitForSpace()
    {
        // Arrange
        var business = BusinessTestData.Create();
        var existingPhotos = Enumerable.Range(1, 4)
            .Select(i => CreateExistingPhoto(business.Id, BusinessPhotoKind.Space, i))
            .ToList();

        _businessRepository.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _businessPhotoRepository
            .Setup(r => r.GetByBusinessIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)existingPhotos);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(business.Id, BusinessPhotoKind.Space));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenBusinessDoesNotExist()
    {
        // Arrange
        var businessId = new BusinessId("business-missing");
        _businessRepository.Setup(r => r.GetByIdAsync(businessId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(businessId, BusinessPhotoKind.Space));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
