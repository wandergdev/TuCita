using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using Xunit;

namespace TuCita.Domain.UnitTests.Entities;

public class PortfolioPhotoTests
{
    private static readonly ProfessionalId ProfessionalId = new("professional-001");

    private static PortfolioPhoto CreatePhoto(string? caption = null)
        => PortfolioPhoto.Create(
            new PortfolioPhotoId("photo-001"),
            ProfessionalId,
            "https://res.cloudinary.com/demo/image/upload/v1/professionals/professional-001/photo.jpg",
            "professionals/professional-001/photo",
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
    public void Create_ShouldTrimCaption()
    {
        // Act
        var photo = CreatePhoto("  Corte fade  ");

        // Assert
        photo.Caption.Should().Be("Corte fade");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenImageUrlIsEmpty()
    {
        // Act
        Action action = () => PortfolioPhoto.Create(
            new PortfolioPhotoId("photo-001"),
            ProfessionalId,
            "",
            "professionals/professional-001/photo");

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenCaptionExceedsMaxLength()
    {
        // Arrange
        var tooLongCaption = new string('a', 201);

        // Act
        Action action = () => CreatePhoto(tooLongCaption);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReplaceImage_ShouldUpdateImageUrlAndPublicId()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        photo.ReplaceImage(
            "https://res.cloudinary.com/demo/image/upload/v2/professionals/professional-001/edited.jpg",
            "professionals/professional-001/edited");

        // Assert
        photo.ImageUrl.Should().Be(
            "https://res.cloudinary.com/demo/image/upload/v2/professionals/professional-001/edited.jpg");
        photo.PublicId.Should().Be("professionals/professional-001/edited");
    }

    [Fact]
    public void ReplaceImage_ShouldThrowException_WhenImageUrlIsEmpty()
    {
        // Arrange
        var photo = CreatePhoto();

        // Act
        Action action = () => photo.ReplaceImage("", "professionals/professional-001/edited");

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
    public void Reject_ShouldThrowException_WhenNotPending()
    {
        // Arrange
        var photo = CreatePhoto();
        photo.Reject();

        // Act
        Action action = () => photo.Reject();

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
