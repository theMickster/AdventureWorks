using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;

namespace AdventureWorks.UnitTests.Application.Features.AddressManagement.Queries;

public sealed class ReadAddressTypeListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressTypeRepository> _mockRepository = new();
    private ReadAddressTypeListQueryHandler _sut;

    public ReadAddressTypeListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadAddressTypeListQueryHandler(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadAddressTypeListQueryHandler(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadAddressTypeListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressTypeRepository");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<AddressTypeEntity>)null!);

        var result = await _sut.Handle(new ReadAddressTypeListQuery(), CancellationToken.None) ;
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<AddressTypeEntity>());

        result = await _sut.Handle(new ReadAddressTypeListQuery(), CancellationToken.None);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_valid_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<AddressTypeEntity>
            {
                new() {AddressTypeId = 1, Name = "Home"}
                ,new() {AddressTypeId = 2, Name = "Billing"}
                ,new() {AddressTypeId = 3, Name = "Mailing"}
            });

        var result = await _sut.Handle(new ReadAddressTypeListQuery(), CancellationToken.None);
        result.Count.Should().Be(3);
    }
}
