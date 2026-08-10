using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using Xunit;

namespace TuCita.Domain.UnitTests.Entities;

public class BusinessPhotoTests
{
    private static readonly BusinessId BusinessId = new("business-001");

    private static BusinessPhoto CreatePhoto(
        BusinessPhotoKind kind = BusinessPhotoKind.Space,
        string? caption = null)
        => BusinessPhoto.Create(
            new BusinessPhotoId("photo-001"),
            BusinessId,
            kind,
            "https://res.cloudinary.com/demo/image/upload/v1/businesses/business-001/photo.jpg",
            "businesses/business-001/photo",
            caption);

    [Fact]
    public void Create_ShouldStartAsPending()
    {
        // Act
        var photo = CreatePhoto();

        // Assert
        photo.Status.Should().Be(PortfolioPhotoStatus.Pending);
    }

    [Fact]
    public void Create_ShouldStoreKind()
    {
        // Act
        var photo = CreatePhoto(BusinessPhotoKind.Cover);

        // Assert
        photo.Kind.Should().Be(BusinessPhotoKind.Cover);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenImageUrlIsEmpty()
    {
        // Act
        Action action = () => BusinessPhoto.Create(
            new BusinessPhotoId("photo-001"),
            BusinessId,
            BusinessPhotoKind.Space,
            "",
            "businesses/business-001/photo");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_ShouldSetStatusToApproved_WhenPending()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        photo.Approve();

        // Assert
        photo.Status.Should().Be(PortfolioPhotoStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldThrowException_WhenNotPending()
    {
        // Arrange
        var photo = CreatePhoto();
        photo.Approve();

        // Act
        Action action = () => photo.Approve();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_ShouldSetStatusToRejected_WhenPending()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        photo.Reject();

        // Assert
        photo.Status.Should().Be(PortfolioPhotoStatus.Rejected);
    }

    [Fact]
    public void ReplaceImage_ShouldUpdateImageUrlAndPublicId()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        photo.ReplaceImage(
            "https://res.cloudinary.com/demo/image/upload/v2/businesses/business-001/edited.jpg",
            "businesses/business-001/edited");

        // Assert
        photo.ImageUrl.Should().Be(
            "https://res.cloudinary.com/demo/image/upload/v2/businesses/business-001/edited.jpg");
        photo.PublicId.Should().Be("businesses/business-001/edited");
    }
}
