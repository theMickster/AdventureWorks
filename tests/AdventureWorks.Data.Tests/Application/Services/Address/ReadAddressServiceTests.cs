using AdventureWorks.Application.Interfaces.Repositories;
using AdventureWorks.Application.Services.Address;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Profiles;
using AdventureWorks.UnitTests.Setup;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.Address;

[ExcludeFromCodeCoverage]
public sealed class ReadAddressServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IAddressRepository> _mockAddressRepository = new();
    private ReadAddressService _sut;

    public ReadAddressServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(AddressEntityToAddressModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadAddressService(_mapper, _mockAddressRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadAddressService(
                    null!,
                    _mockAddressRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadAddressService(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("addressRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((AddressEntity)null!);

        var result = await _sut.GetByIdAsync(7).ConfigureAwait(false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_valid_model_Async()
    {
        const int addressId = 797;
        var dateModified = new DateTime(2011, 11, 11);

        var addressEntity = new AddressEntity
        {
            AddressId = addressId,
            AddressLine1 = "1234",
            AddressLine2 = "5671",
            City = "Denver",
            ModifiedDate = dateModified,
            PostalCode = "90210",
            Rowguid = new Guid("a3220216-5a3b-4036-b184-d23e0f811b47"),
            StateProvinceId = 18,
            StateProvince = new StateProvinceEntity
            {
                StateProvinceId = 18,
                CountryRegionCode = "US",
                Name = "Colorado",
                CountryRegion = new CountryRegionEntity
                {
                    CountryRegionCode = "US",
                    Name = "United States"
                }
            }
        };

        _mockAddressRepository.Setup(x => x.GetAddressByIdAsync(addressId))
            .ReturnsAsync(addressEntity);

        var result = await _sut.GetByIdAsync(addressId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();

            result!.Id.Should().Be(addressId);

            result.AddressLine1.Should().Be("1234");
            result.AddressLine2.Should().Be("5671");
            result.City.Should().Be("Denver");
            result.StateProvince.Id.Should().Be(18);
            result.ModifiedDate.Should().Be(dateModified);
            result.ModifiedDate.Year.Should().Be(dateModified.Year);
            result.CountryRegion.CountryRegionName.Should().Be("United States");
        }
    }
}
