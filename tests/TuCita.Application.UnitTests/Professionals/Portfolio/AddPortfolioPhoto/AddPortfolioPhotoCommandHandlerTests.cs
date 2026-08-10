using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.Portfolio.AddPortfolioPhoto;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.Portfolio.AddPortfolioPhoto;

public class AddPortfolioPhotoCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IPortfolioPhotoRepository> _portfolioPhotoRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly AddPortfolioPhotoCommandHandler _handler;

    public AddPortfolioPhotoCommandHandlerTests()
    {
        _handler = new AddPortfolioPhotoCommandHandler(
            _professionalRepository.Object,
            _portfolioPhotoRepository.Object,
            _identifierGenerator.Object);
    }

    private static AddPortfolioPhotoCommand CreateCommand(ProfessionalId professionalId) => new(
        professionalId,
        "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
        "professionals/professional-001/photo",
        "Corte fade");

    [Fact]
    public async Task HandleAsync_ShouldAddPhoto_WhenUnderTheLimit()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var photoId = new PortfolioPhotoId("photo-001");

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _portfolioPhotoRepository
            .Setup(r => r.GetByProfessionalIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PortfolioPhoto>)[]);
        _identifierGenerator.Setup(g => g.NewPortfolioPhotoId()).Returns(photoId);

        // Act
        var response = await _handler.HandleAsync(CreateCommand(professional.Id));

        // Assert
        response.PortfolioPhotoId.Should().Be(photoId);
        _portfolioPhotoRepository.Verify(
            r => r.AddAsync(
                It.Is<PortfolioPhoto>(p => p.ProfessionalId == professional.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAtTheLimit()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var existingPhotos = Enumerable.Range(1, 2)
            .Select(i => PortfolioPhoto.Create(
                new PortfolioPhotoId($"photo-{i}"),
                professional.Id,
                "https://res.cloudinary.com/demo/image/upload/v1/photo.jpg",
                $"professionals/professional-001/photo-{i}"))
            .ToList();

        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _portfolioPhotoRepository
            .Setup(r => r.GetByProfessionalIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PortfolioPhoto>)existingPhotos);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(professional.Id));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
        _portfolioPhotoRepository.Verify(
            r => r.AddAsync(It.IsAny<PortfolioPhoto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        var professionalId = new ProfessionalId("professional-missing");
        _professionalRepository.Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(professionalId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
