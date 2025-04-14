using AdventureWorks.Application.Features.AddressManagement.Contracts;
using AdventureWorks.Application.Features.AddressManagement.Profiles;
using AdventureWorks.Application.Features.AddressManagement.Services.AddressType;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.AddressType;

[ExcludeFromCodeCoverage]
public sealed class ReadAddressTypeServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressTypeRepository> _mockRepository = new();
    private ReadAddressTypeService _sut;

    public ReadAddressTypeServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressTypeEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadAddressTypeService(_mapper, _mockRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadAddressTypeService)
                .Should().Implement<IReadAddressTypeService>();

            typeof(ReadAddressTypeService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadAddressTypeService(
                    null!,
                    _mockRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadAddressTypeService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressTypeRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AddressTypeEntity)null!);

        var result = await _sut.GetByIdAsync(12);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_correctly_Async()
    {
        _mockRepository.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new AddressTypeEntity{AddressTypeId = 1, Name = "Home"});

        var result = await _sut.GetByIdAsync(1);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.Name.Should().Be("Home");
        }
    }

    [Fact]
    public async Task GetListAsync_returns_empty_listAsync()
    {
        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync((IReadOnlyList<AddressTypeEntity>)null!);

        var result = await _sut.GetListAsync();
        result.Should().BeEmpty();

        _mockRepository.Reset();

        _mockRepository.Setup(x => x.ListAllAsync())
            .ReturnsAsync(new List<AddressTypeEntity>());

        result = await _sut.GetListAsync();
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

        var result = await _sut.GetListAsync();
        result.Count.Should().Be(3);
    }
}
