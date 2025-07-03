using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

public sealed class ReadDepartmentListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IDepartmentRepository> _mockRepository = new();
    private ReadDepartmentListQueryHandler _sut;

    public ReadDepartmentListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(DepartmentEntityToDepartmentModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadDepartmentListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadDepartmentListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadDepartmentListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("departmentRepository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<DepartmentEntity>)null!);

        var result = await _sut.Handle(new ReadDepartmentListQuery(), CancellationToken.None);
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<DepartmentEntity>());

        result = await _sut.Handle(new ReadDepartmentListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<DepartmentEntity>
            {
                new() { DepartmentId = 1, Name = "Engineering", GroupName = "Research and Development" }
                ,new() { DepartmentId = 2, Name = "Tool Design", GroupName = "Research and Development" }
                ,new() { DepartmentId = 3, Name = "Sales", GroupName = "Sales and Marketing" }
                ,new() { DepartmentId = 4, Name = "Marketing", GroupName = "Sales and Marketing" }
            });

        var result = await _sut.Handle(new ReadDepartmentListQuery(), CancellationToken.None);
        result.Count.Should().Be(4);
    }
}
