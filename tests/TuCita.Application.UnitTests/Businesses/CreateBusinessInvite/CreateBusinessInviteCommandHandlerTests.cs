using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.CreateBusinessInvite;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.CreateBusinessInvite;

public class CreateBusinessInviteCommandHandlerTests
{
    private readonly Mock<IBusinessInviteRepository> _businessInviteRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly CreateBusinessInviteCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);
    private static readonly BusinessId OwnerId = new("business-001");

    public CreateBusinessInviteCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        var frontendOptions = Microsoft.Extensions.Options.Options.Create(
            new FrontendOptions { BaseUrl = "https://tucita.example" });

        _handler = new CreateBusinessInviteCommandHandler(
            _businessInviteRepository.Object,
            _identifierGenerator.Object,
            _dateTimeProvider.Object,
            frontendOptions);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateInviteAndReturnUrl()
    {
        // Arrange
        var inviteId = new BusinessInviteId("invite-001");
        _identifierGenerator.Setup(g => g.NewBusinessInviteId()).Returns(inviteId);

        // Act
        var response = await _handler.HandleAsync(new CreateBusinessInviteCommand(OwnerId));

        // Assert
        response.InviteUrl.Should().StartWith("https://tucita.example/registro?invite=");
        _businessInviteRepository.Verify(r => r.AddAsync(
            It.Is<BusinessInvite>(i => i.BusinessId == OwnerId && i.IsValid(Now)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
