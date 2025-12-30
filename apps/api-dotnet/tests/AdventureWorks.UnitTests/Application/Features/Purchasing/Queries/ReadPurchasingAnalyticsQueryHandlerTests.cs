using AdventureWorks.Application.Features.Purchasing.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Purchasing;
using AdventureWorks.Models.Features.Purchasing;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Purchasing.Queries;

public sealed class ReadPurchasingAnalyticsQueryHandlerTests
{
    private readonly Mock<IPurchasingAnalyticsRepository> _mockPurchasingAnalyticsRepository = new();
    private readonly ReadPurchasingAnalyticsQueryHandler _sut;

    public ReadPurchasingAnalyticsQueryHandlerTests()
    {
        _sut = new ReadPurchasingAnalyticsQueryHandler(_mockPurchasingAnalyticsRepository.Object);
    }

    [Fact]
    public void Handler_throws_correct_exception_when_repository_is_null()
    {
        _ = ((Action)(() => _ = new ReadPurchasingAnalyticsQueryHandler(null!)))
            .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
            .And.ParamName.Should().Be("purchasingAnalyticsRepository");
    }

    [Fact]
    public async Task Handle_throws_correct_exception_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_repository_model_unchanged()
    {
        // Arrange
        var analytics = new PurchasingAnalyticsModel
        {
            ParetoData =
            [
                new VendorSpendModel { VendorId = 1576, VendorName = "Superior Bicycles", TotalSpend = 5034266.74m, CumulativePercent = 12.5m },
                new VendorSpendModel { VendorId = 1496, VendorName = "Advanced Bicycles", TotalSpend = 762.94m, CumulativePercent = 100m }
            ],
            PipelineSummary =
            [
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Pending", PoCount = 225, TotalValue = 1000m },
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Approved", PoCount = 12, TotalValue = 2000m },
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Rejected", PoCount = 86, TotalValue = 3000m },
                new PurchaseOrderPipelineStatusModel { StatusLabel = "Complete", PoCount = 3689, TotalValue = 4000m }
            ]
        };

        _mockPurchasingAnalyticsRepository
            .Setup(x => x.GetPurchasingAnalyticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

        // Act
        var result = await _sut.Handle(new ReadPurchasingAnalyticsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(analytics);
    }

    [Fact]
    public async Task Handle_forwards_the_cancellation_token_to_the_repository()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        _mockPurchasingAnalyticsRepository
            .Setup(x => x.GetPurchasingAnalyticsAsync(cancellationTokenSource.Token))
            .ReturnsAsync(new PurchasingAnalyticsModel());

        // Act
        await _sut.Handle(new ReadPurchasingAnalyticsQuery(), cancellationTokenSource.Token);

        // Assert
        _mockPurchasingAnalyticsRepository.Verify(
            x => x.GetPurchasingAnalyticsAsync(cancellationTokenSource.Token),
            Times.Once);
    }
}
