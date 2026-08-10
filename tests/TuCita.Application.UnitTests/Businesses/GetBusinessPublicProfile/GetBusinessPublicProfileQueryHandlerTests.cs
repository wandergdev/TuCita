using FluentAssertions;
using Moq;
using TuCita.Application.Businesses.GetBusinessPublicProfile;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.GetBusinessPublicProfile;

public class GetBusinessPublicProfileQueryHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IPortfolioPhotoRepository> _portfolioPhotoRepository = new();
    private readonly Mock<IBusinessPhotoRepository> _businessPhotoRepository = new();
    private readonly GetBusinessPublicProfileQueryHandler _handler;

    public GetBusinessPublicProfileQueryHandlerTests()
    {
        _handler = new GetBusinessPublicProfileQueryHandler(
            _businessRepository.Object,
            _professionalRepository.Object,
            _portfolioPhotoRepository.Object,
            _businessPhotoRepository.Object);
    }

    private void SetUpEmptyBusinessPhotos(BusinessId businessId)
    {
        _businessPhotoRepository
            .Setup(r => r.GetApprovedByBusinessIdAsync(businessId, BusinessPhotoKind.Cover, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)[]);
        _businessPhotoRepository
            .Setup(r => r.GetApprovedByBusinessIdAsync(businessId, BusinessPhotoKind.Space, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)[]);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBusinessWithItsProfessionals()
    {
        // Arrange
        var business = BusinessTestData.Create();
        var professional = ProfessionalTestData.Create();
        professional.JoinBusiness(business.Id);

        _businessRepository.Setup(r => r.GetBySlugAsync(business.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _professionalRepository.Setup(r => r.GetByBusinessIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Professional>)[professional]);
        _portfolioPhotoRepository
            .Setup(r => r.GetApprovedByProfessionalIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PortfolioPhoto>)[]);
        SetUpEmptyBusinessPhotos(business.Id);

        // Act
        var response = await _handler.HandleAsync(new GetBusinessPublicProfileQuery(business.Slug.Value));

        // Assert
        response.Name.Should().Be(business.Name);
        response.Professionals.Should().ContainSingle();
        response.Professionals.First().Slug.Should().Be(professional.Slug.Value);
        response.Professionals.First().CoverPhotoUrls.Should().BeEmpty();
        response.CoverImageUrl.Should().BeNull();
        response.SpacePhotos.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCoverImageAndSpacePhotos()
    {
        // Arrange
        var business = BusinessTestData.Create();

        var coverPhoto = BusinessPhoto.Create(
            new BusinessPhotoId("photo-cover"),
            business.Id,
            BusinessPhotoKind.Cover,
            "https://res.cloudinary.com/demo/image/upload/v1/cover.jpg",
            "businesses/business-001/cover");
        coverPhoto.Approve();

        var spacePhoto = BusinessPhoto.Create(
            new BusinessPhotoId("photo-space"),
            business.Id,
            BusinessPhotoKind.Space,
            "https://res.cloudinary.com/demo/image/upload/v1/space.jpg",
            "businesses/business-001/space");
        spacePhoto.Approve();

        _businessRepository.Setup(r => r.GetBySlugAsync(business.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _professionalRepository.Setup(r => r.GetByBusinessIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Professional>)[]);
        _businessPhotoRepository
            .Setup(r => r.GetApprovedByBusinessIdAsync(business.Id, BusinessPhotoKind.Cover, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)[coverPhoto]);
        _businessPhotoRepository
            .Setup(r => r.GetApprovedByBusinessIdAsync(business.Id, BusinessPhotoKind.Space, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<BusinessPhoto>)[spacePhoto]);

        // Act
        var response = await _handler.HandleAsync(new GetBusinessPublicProfileQuery(business.Slug.Value));

        // Assert
        response.CoverImageUrl.Should().Be(coverPhoto.ImageUrl);
        response.SpacePhotos.Should().Equal(spacePhoto.ImageUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnCoverPhotosMostRecentFirst()
    {
        // Arrange
        var business = BusinessTestData.Create();
        var professional = ProfessionalTestData.Create();
        professional.JoinBusiness(business.Id);

        var olderPhoto = PortfolioPhoto.Create(
            new PortfolioPhotoId("photo-older"),
            professional.Id,
            "https://res.cloudinary.com/demo/image/upload/v1/older.jpg",
            "professionals/professional-001/older");
        olderPhoto.Approve();

        Thread.Sleep(5);

        var newerPhoto = PortfolioPhoto.Create(
            new PortfolioPhotoId("photo-newer"),
            professional.Id,
            "https://res.cloudinary.com/demo/image/upload/v1/newer.jpg",
            "professionals/professional-001/newer");
        newerPhoto.Approve();

        _businessRepository.Setup(r => r.GetBySlugAsync(business.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _professionalRepository.Setup(r => r.GetByBusinessIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Professional>)[professional]);
        _portfolioPhotoRepository
            .Setup(r => r.GetApprovedByProfessionalIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PortfolioPhoto>)[olderPhoto, newerPhoto]);
        SetUpEmptyBusinessPhotos(business.Id);

        // Act
        var response = await _handler.HandleAsync(new GetBusinessPublicProfileQuery(business.Slug.Value));

        // Assert
        response.Professionals.First().CoverPhotoUrls.Should().Equal(newerPhoto.ImageUrl, olderPhoto.ImageUrl);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenBusinessDoesNotExist()
    {
        // Arrange
        _businessRepository.Setup(r => r.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        // Act
        var action = () => _handler.HandleAsync(new GetBusinessPublicProfileQuery("missing-slug"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
