using AdventureWorks.Application.Features.Production.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Models.Features.Production;
using FluentAssertions;
using Moq;

namespace AdventureWorks.UnitTests.Application.Features.Production.Queries;

public sealed class ReadManufacturingQualityScorecardQueryHandlerTests
{
    private readonly Mock<IManufacturingQualityScorecardRepository> _mockRepository = new();
    private readonly ReadManufacturingQualityScorecardQueryHandler _sut;

    public ReadManufacturingQualityScorecardQueryHandlerTests()
    {
        _sut = new ReadManufacturingQualityScorecardQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public void Handler_throws_correct_exception_when_repository_is_null()
    {
        _ = ((Action)(() => _ = new ReadManufacturingQualityScorecardQueryHandler(null!)))
            .Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("qualityScorecardRepository");
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
        var scorecard = new ManufacturingQualityScorecardModel
        {
            Top5ByScrapped = [new QualityScorecardProductModel { ProductId = 1 }]
        };
        _mockRepository
            .Setup(repository => repository.GetQualityScorecardAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(scorecard);

        var result = await _sut.Handle(new ReadManufacturingQualityScorecardQuery(), CancellationToken.None);

        result.Should().BeSameAs(scorecard);
    }

    [Fact]
    public async Task Handle_forwards_the_cancellation_token_to_the_repository()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        _mockRepository
            .Setup(repository => repository.GetQualityScorecardAsync(cancellationTokenSource.Token))
            .ReturnsAsync(new ManufacturingQualityScorecardModel());

        await _sut.Handle(new ReadManufacturingQualityScorecardQuery(), cancellationTokenSource.Token);

        _mockRepository.Verify(
            repository => repository.GetQualityScorecardAsync(cancellationTokenSource.Token),
            Times.Once);
    }
}
