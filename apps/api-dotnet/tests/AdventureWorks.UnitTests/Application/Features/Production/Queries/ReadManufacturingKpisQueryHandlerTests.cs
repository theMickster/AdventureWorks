using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadManufacturingKpisQueryHandlerTests
{
    private readonly Mock<IManufacturingKpiRepository> _mockManufacturingKpiRepository = new();
    private readonly ReadManufacturingKpisQueryHandler _sut;

    public ReadManufacturingKpisQueryHandlerTests()
    {
        _sut = new ReadManufacturingKpisQueryHandler(_mockManufacturingKpiRepository.Object);
    }

    [Fact]
    public void Handler_throws_correct_exception_when_repository_is_null()
    {
        _ = ((Action)(() => _ = new ReadManufacturingKpisQueryHandler(null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("manufacturingKpiRepository");
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
        var kpis = new ManufacturingKpisModel
        {
            TotalWorkOrders = 72591,
            TotalOrdered = 100000,
            TotalStocked = 99000,
            TotalScrapped = 1000,
            OverallYieldPct = 99m,
            OverallScrapPct = 1m
        };

        _mockManufacturingKpiRepository
            .Setup(x => x.GetManufacturingKpisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(kpis);

        var result = await _sut.Handle(new ReadManufacturingKpisQuery(), CancellationToken.None);

        result.Should().BeSameAs(kpis);
    }

    [Fact]
    public async Task Handle_forwards_the_cancellation_token_to_the_repository()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        _mockManufacturingKpiRepository
            .Setup(x => x.GetManufacturingKpisAsync(cancellationTokenSource.Token))
            .ReturnsAsync(new ManufacturingKpisModel());

        await _sut.Handle(new ReadManufacturingKpisQuery(), cancellationTokenSource.Token);

        _mockManufacturingKpiRepository.Verify(
            x => x.GetManufacturingKpisAsync(cancellationTokenSource.Token),
            Times.Once);
    }
}
