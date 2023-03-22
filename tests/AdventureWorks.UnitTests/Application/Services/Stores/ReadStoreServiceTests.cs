using AdventureWorks.Application.Interfaces.Repositories.Person;
using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Application.Interfaces.Services.Stores;
using AdventureWorks.Application.Services.Stores;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Filtering;
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

        var entity = GetMockStores().FirstOrDefault(x => x.BusinessEntityId == storeId);

        var storeContacts = 
            GetMockContactEntities().Where(x => x.BusinessEntityId == storeId).ToList();

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
            result.ModifiedDate.Should().Be(DefaultAuditDate);

            result.StoreContacts.Should().HaveCount(2);

            result.StoreContacts.Count(x => x.FirstName == "Steve").Should().Be(1);
            result.StoreContacts.Count(x => x.LastName == "Jones").Should().Be(2);
        }
    }

    [Fact]
    public async Task GetStoresAsync_returns_correct_null_resultAsync()
    {
        _mockStoreRepository.Setup(x => x.GetStoresAsync(It.IsAny<StoreParameter>()))
            .ReturnsAsync((null!, 0) );

        var result = await _sut.GetStoresAsync(new StoreParameter()).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();

            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetStoresAsync_returns_correct_empty_resultAsync()
    {
        var readOnlyList = new List<StoreEntity>().AsReadOnly();
        _mockStoreRepository.Setup(x => x.GetStoresAsync(It.IsAny<StoreParameter>()))
            .ReturnsAsync( (readOnlyList, 0));

        var result = await _sut.GetStoresAsync(new StoreParameter()).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();

            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetStoresAsync_returns_valid_paged_model_Async()
    {
        _mockStoreRepository.Setup(x => x.GetStoresAsync(It.IsAny<StoreParameter>()))
            .ReturnsAsync((GetMockStores().ToList(), 3));

        _mockContactEntityRepository.Setup(x => x.GetContactsByStoreIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(GetMockContactEntities().ToList());

        var pagedResult = await _sut.GetStoresAsync(new StoreParameter()
            { PageNumber = 1, OrderBy = "Name", PageSize = 30, SortOrder = "DESCENDING" }).ConfigureAwait(false);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();

            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(30);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(3);

            pagedResult.Results.Should().HaveCount(3);

            var store01 = pagedResult.Results.FirstOrDefault(x => x.Id == 2534);
            var store02 = pagedResult.Results.FirstOrDefault(x => x.Id == 2535);

            store01!.Should().NotBeNull();

            store01!.StoreContacts.Should().HaveCount(2);
            store02!.StoreContacts.Should().HaveCount(3);
            store02!.StoreAddresses.Should().HaveCount(4);
        }
    }
    
    #region Private Methods

    private IEnumerable<StoreEntity> GetMockStores()
    {
        var usa = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States of America", ModifiedDate = DefaultAuditDate };
        var colorado = new StateProvinceEntity { StateProvinceId = 10, Name = "Colorado", CountryRegionCode = "US", CountryRegion = usa };
        
        return new List<StoreEntity>
        {
            new()
            {
                BusinessEntityId = 2534,
                Name = "Pro Sporting Goods",
                SalesPersonId = 7777,
                PrimarySalesPerson = new SalesPerson { TerritoryId = 7, BusinessEntityId = 7777 },
                ModifiedDate = DefaultAuditDate,
                StoreBusinessEntity = new BusinessEntity
                {
                    BusinessEntityId = 2534, BusinessEntityAddresses = new List<BusinessEntityAddressEntity>()
                    {
                        new()
                        {
                            BusinessEntityId = 2534,
                            Address = new AddressEntity
                            {
                                AddressId = 553,
                                AddressLine1 = "1234 Broadway Ave",
                                City = "Aurora",
                                StateProvinceId = 10,
                                Rowguid = new Guid("8f83c8eb-ee79-46ba-8e2c-6645794674b4"),
                                PostalCode = "80015",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 1,
                            AddressType = new AddressTypeEntity { AddressTypeId = 1, Name = "Home" }

                        },
                        new()
                        {
                            BusinessEntityId = 2534,
                            Address = new AddressEntity
                            {
                                AddressId = 554,
                                AddressLine1 = "456 Union Ave",
                                City = "Aurora",
                                StateProvinceId = 10,
                                Rowguid = new Guid("28238dcb-1842-4a64-9224-6b13cbbb0ab4"),
                                PostalCode = "80016",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 2,
                            AddressType = new AddressTypeEntity { AddressTypeId = 2, Name = "Billing" }
                        },
                    }
                }
            },
            new()
            {
                BusinessEntityId = 2535,
                Name = "Semi-Pro Sporting Goods",
                SalesPersonId = 7778,
                PrimarySalesPerson = new SalesPerson { TerritoryId = 7, BusinessEntityId = 7778 },
                ModifiedDate = DefaultAuditDate,
                StoreBusinessEntity = new BusinessEntity
                {
                    BusinessEntityId = 2535, BusinessEntityAddresses = new List<BusinessEntityAddressEntity>()
                    {
                        new()
                        {
                            BusinessEntityId = 2535,
                            Address = new AddressEntity
                            {
                                AddressId = 555,
                                AddressLine1 = "25981 College Street",
                                City = "Montreal",
                                StateProvinceId = 10,
                                Rowguid = new Guid("11F4E4E8-CABA-4B5A-991A-909ABCFF61AF"),
                                PostalCode = "80010",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 1,
                            AddressType = new AddressTypeEntity { AddressTypeId = 1, Name = "Home" }

                        },
                        new()
                        {
                            BusinessEntityId = 2535,
                            Address = new AddressEntity
                            {
                                AddressId = 556,
                                AddressLine1 = "26910 Indela Road",
                                City = "Aurora",
                                StateProvinceId = 10,
                                Rowguid = new Guid("8612FBAD-3CE1-435B-BE0B-5A48DDBD2B62"),
                                PostalCode = "80011",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 2,
                            AddressType = new AddressTypeEntity { AddressTypeId = 2, Name = "Billing" }
                        },
                        new()
                        {
                            BusinessEntityId = 2535,
                            Address = new AddressEntity
                            {
                                AddressId = 557,
                                AddressLine1 = "2551 East Warner Road",
                                City = "Aurora",
                                StateProvinceId = 10,
                                Rowguid = new Guid("54F6FA82-5762-4C27-B0FE-5A30E06C57B2"),
                                PostalCode = "80012",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 2,
                            AddressType = new AddressTypeEntity { AddressTypeId = 2, Name = "Billing" }
                        },
                        new()
                        {
                            BusinessEntityId = 2535,
                            Address = new AddressEntity
                            {
                                AddressId = 558,
                                AddressLine1 = "998 Forest Road",
                                City = "Aurora",
                                StateProvinceId = 10,
                                Rowguid = new Guid("4DDBACC9-7898-403E-856A-128E34245424"),
                                PostalCode = "80013",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 2,
                            AddressType = new AddressTypeEntity { AddressTypeId = 2, Name = "Billing" }
                        },
                    }
                }
            },
            new()
            {
                BusinessEntityId = 2536,
                Name = "Rookie Sporting Goods",
                SalesPersonId = 7779,
                PrimarySalesPerson = new SalesPerson { TerritoryId = 7, BusinessEntityId = 7779 },
                ModifiedDate = DefaultAuditDate,
                StoreBusinessEntity = new BusinessEntity
                {
                    BusinessEntityId = 2536, BusinessEntityAddresses = new List<BusinessEntityAddressEntity>()
                    {
                        new()
                        {
                            BusinessEntityId = 2536,
                            Address = new AddressEntity
                            {
                                AddressId = 559,
                                AddressLine1 = "254 Colonnade Road",
                                City = "Montreal",
                                StateProvinceId = 10,
                                Rowguid = new Guid("8EC24E8C-C114-4240-80D5-CE7E4BDAE4D1"),
                                PostalCode = "80010",
                                ModifiedDate = DefaultAuditDate,
                                StateProvince = colorado
                            },
                            AddressTypeId = 3,
                            AddressType = new AddressTypeEntity { AddressTypeId = 3, Name = "Main Office" }

                        }
                    }
                }
            }
        };
    }

    private IEnumerable<BusinessEntityContactEntity> GetMockContactEntities()
    {
        return new List<BusinessEntityContactEntity>
        {
            new()
            {
                BusinessEntityId = 2534, ContactTypeId = 11,
                ContactType = new ContactTypeEntity { ContactTypeId = 11, Name = "Owner" }, PersonId = 987,
                Person = new PersonEntity { BusinessEntityId = 987, FirstName = "Steve", LastName = "Jones" }
            },
            new()
            {
                BusinessEntityId = 2534, ContactTypeId = 12,
                ContactType = new ContactTypeEntity { ContactTypeId = 12, Name = "Store Contact" }, PersonId = 988,
                Person = new PersonEntity { BusinessEntityId = 988, FirstName = "Peter", LastName = "Jones" }
            },
            new()
            {
                BusinessEntityId = 2535, ContactTypeId = 11,
                ContactType = new ContactTypeEntity { ContactTypeId = 11, Name = "Owner" }, PersonId = 989,
                Person = new PersonEntity { BusinessEntityId = 989, FirstName = "Amy", LastName = "Alberts" }
            },
            new()
            {
                BusinessEntityId = 2535, ContactTypeId = 12,
                ContactType = new ContactTypeEntity { ContactTypeId = 12, Name = "Store Contact" }, PersonId = 990,
                Person = new PersonEntity { BusinessEntityId = 990, FirstName = "Emilio", LastName = "Alvarado" }
            },
            new()
            {
                BusinessEntityId = 2535, ContactTypeId = 12,
                ContactType = new ContactTypeEntity { ContactTypeId = 12, Name = "Store Contact" }, PersonId = 991,
                Person = new PersonEntity { BusinessEntityId = 991, FirstName = "Oscar", LastName = "Belli" }
            },
            new()
            {
                BusinessEntityId = 2536, ContactTypeId = 11,
                ContactType = new ContactTypeEntity { ContactTypeId = 11, Name = "Store Contact" }, PersonId = 992,
                Person = new PersonEntity { BusinessEntityId = 992, FirstName = "Payton", LastName = "Benson" }
            }
        };
    }

    #endregion Private Methods
}
