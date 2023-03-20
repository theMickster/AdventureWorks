using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Application.Services.Stores;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Profiles.Sales;
using AutoMapper;

namespace AdventureWorks.UnitTests.Application.Services.Stores;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreServiceTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockContactEntityRepository = new();
    private ReadStoreService _sut;

    public ReadStoreServiceTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreService(_mapper, _mockStoreRepository.Object, _mockContactEntityRepository.Object);
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(ReadStoreService)
                .Should().Implement<IReadStoreService>();

            typeof(ReadStoreService)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreService(
                    null!,
                    _mockStoreRepository.Object,
                    _mockContactEntityRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreService(
                    _mapper,
                    null!,
                    _mockContactEntityRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("storeRepository");

            _ = ((Action)(() => _sut = new ReadStoreService(
                    _mapper,
                    _mockStoreRepository.Object,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("businessEntityContactEntityRepository");
        }
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_Async()
    {
        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((StoreEntity)null!);

        var result = await _sut.GetByIdAsync(22).ConfigureAwait(false);

        result?.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_valid_model_Async()
    {
        const int storeId = 2534;
        var dateModified = new DateTime(2011, 11, 11);
        var usa = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States of America", ModifiedDate = dateModified };
        var colorado = new StateProvinceEntity { StateProvinceId = 10, Name = "Colorado", CountryRegionCode = "US", CountryRegion = usa };

        var entity = new StoreEntity
        {
            BusinessEntityId = storeId,
            Name = "Pro Sporting Goods",
            SalesPersonId = 7777,
            PrimarySalesPerson = new SalesPerson { TerritoryId = 7, BusinessEntityId = 7777 },
            ModifiedDate = dateModified,
            StoreBusinessEntity = new BusinessEntity
            {
                BusinessEntityId = storeId, BusinessEntityAddresses = new List<BusinessEntityAddressEntity>()
                {
                    new()
                    {
                        BusinessEntityId = storeId,
                        Address = new AddressEntity
                        {
                            AddressId = 553,
                            AddressLine1 = "1234 Broadway Ave",
                            City = "Aurora",
                            StateProvinceId = 10,
                            Rowguid = new Guid("8f83c8eb-ee79-46ba-8e2c-6645794674b4"),
                            PostalCode = "80015",
                            ModifiedDate = dateModified,
                            StateProvince = colorado
                        },
                        AddressTypeId = 1,
                        AddressType = new AddressTypeEntity{ AddressTypeId = 1, Name = "Home" }

                    },
                    new()
                    {
                        BusinessEntityId = storeId,
                        Address = new AddressEntity
                        {
                            AddressId = 554,
                            AddressLine1 = "456 Union Ave",
                            City = "Aurora",
                            StateProvinceId = 10,
                            Rowguid = new Guid("28238dcb-1842-4a64-9224-6b13cbbb0ab4"),
                            PostalCode = "80016",
                            ModifiedDate = dateModified,
                            StateProvince = colorado
                        },
                        AddressTypeId = 2,
                        AddressType = new AddressTypeEntity{ AddressTypeId = 2, Name = "Billing" }
                    },
                }
            }
        };

        var storeContacts = new List<BusinessEntityContactEntity>
        {
            new()
            {
                BusinessEntityId = storeId, ContactTypeId = 11,
                ContactType = new ContactTypeEntity { ContactTypeId = 11, Name = "Owner" }, PersonId = 987,
                Person = new PersonEntity { BusinessEntityId = 987, FirstName = "Steve", LastName = "Jones" }
            },
            new()
            {
                BusinessEntityId = storeId, ContactTypeId = 12,
                ContactType = new ContactTypeEntity { ContactTypeId = 12, Name = "Store Contact" }, PersonId = 988,
                Person = new PersonEntity { BusinessEntityId = 988, FirstName = "Peter", LastName = "Jones" }
            },
        };

        _mockStoreRepository.Setup(x => x.GetStoreByIdAsync(storeId))
            .ReturnsAsync(entity);

        _mockContactEntityRepository.Setup(x => x.GetContactsByIdAsync(storeId))
            .ReturnsAsync(storeContacts);

        var result = await _sut.GetByIdAsync(storeId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result!.Should().NotBeNull();
            result!.Id.Should().Be(storeId);
            result.StoreAddresses.Should().HaveCount(2);
            result.ModifiedDate.Should().Be(dateModified);

            result.StoreContacts.Should().HaveCount(2);

            result.StoreContacts.Count(x => x.FirstName == "Steve").Should().Be(1);
            result.StoreContacts.Count(x => x.LastName == "Jones").Should().Be(2);
        }
    }
}
