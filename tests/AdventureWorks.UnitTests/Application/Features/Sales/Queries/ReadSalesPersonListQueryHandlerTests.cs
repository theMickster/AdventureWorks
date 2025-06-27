using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

public sealed class ReadSalesPersonListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private ReadSalesPersonListQueryHandler _sut;

    public ReadSalesPersonListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(SalesPersonEntityToModelProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadSalesPersonListQueryHandler(_mapper, _mockSalesPersonRepository.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadSalesPersonListQueryHandler(
                    null!,
                    _mockSalesPersonRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadSalesPersonListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("salesPersonRepository");
        }
    }

    [Fact]
    public async Task Handle_list_returns_correct_null_resultAsync()
    {
        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonsAsync(It.IsAny<SalesPersonParameter>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(new ReadSalesPersonListQuery { Parameters = new SalesPersonParameter() }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_list_returns_correct_empty_resultAsync()
    {
        var readOnlyList = new List<SalesPersonEntity>().AsReadOnly();
        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonsAsync(It.IsAny<SalesPersonParameter>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(new ReadSalesPersonListQuery { Parameters = new SalesPersonParameter() }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_list_returns_valid_paged_model_Async()
    {
        var mockSalesPersons = GetMockSalesPersonEntities();
        _mockSalesPersonRepository.Setup(x => x.GetSalesPersonsAsync(It.IsAny<SalesPersonParameter>()))
            .ReturnsAsync((mockSalesPersons, 3));

        var param = new SalesPersonParameter { PageNumber = 1, OrderBy = "firstName", PageSize = 30, SortOrder = "DESCENDING" };

        var pagedResult = await _sut.Handle(new ReadSalesPersonListQuery { Parameters = param }, CancellationToken.None);

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

            var salesPerson01 = pagedResult.Results.FirstOrDefault(x => x.Id == 274);
            var salesPerson02 = pagedResult.Results.FirstOrDefault(x => x.Id == 275);

            salesPerson01!.Should().NotBeNull();
            salesPerson01!.FirstName.Should().Be("Stephen");
            salesPerson01!.LastName.Should().Be("Jiang");

            salesPerson02!.Should().NotBeNull();
            salesPerson02!.FirstName.Should().Be("Michael");
            salesPerson02!.LastName.Should().Be("Blythe");
        }
    }

    [Fact]
    public async Task Handle_search_returns_correct_null_resultAsync()
    {
        _mockSalesPersonRepository.Setup(x => x.SearchSalesPersonsAsync(It.IsAny<SalesPersonParameter>(), It.IsAny<SalesPersonSearchModel>()))
            .ReturnsAsync((null!, 0));

        var result = await _sut.Handle(new ReadSalesPersonListQuery { Parameters = new SalesPersonParameter(), SearchModel = new SalesPersonSearchModel() }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_search_returns_correct_empty_resultAsync()
    {
        var readOnlyList = new List<SalesPersonEntity>().AsReadOnly();
        _mockSalesPersonRepository.Setup(x => x.SearchSalesPersonsAsync(It.IsAny<SalesPersonParameter>(), It.IsAny<SalesPersonSearchModel>()))
            .ReturnsAsync((readOnlyList, 0));

        var result = await _sut.Handle(new ReadSalesPersonListQuery { Parameters = new SalesPersonParameter(), SearchModel = new SalesPersonSearchModel() }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Results?.Should().BeNull();
            result.TotalRecords.Should().Be(0);
            result.TotalPages.Should().Be(0);
        }
    }

    [Fact]
    public async Task Handle_search_returns_valid_paged_model_Async()
    {
        var mockSalesPersons = GetMockSalesPersonEntities();
        _mockSalesPersonRepository.Setup(x => x.SearchSalesPersonsAsync(It.IsAny<SalesPersonParameter>(), It.IsAny<SalesPersonSearchModel>()))
            .ReturnsAsync((mockSalesPersons.Where(x => x.BusinessEntityId == 274).ToList(), 1));

        var queryParam = new SalesPersonParameter { PageNumber = 1, OrderBy = "lastName", PageSize = 10, SortOrder = "ASC" };
        var searchParam = new SalesPersonSearchModel { Id = 274 };

        var pagedResult = await _sut.Handle(new ReadSalesPersonListQuery { Parameters = queryParam, SearchModel = searchParam }, CancellationToken.None);

        using (new AssertionScope())
        {
            pagedResult.Should().NotBeNull();
            pagedResult.PageNumber.Should().Be(1);
            pagedResult.PageSize.Should().Be(10);
            pagedResult.HasPreviousPage.Should().BeFalse();
            pagedResult.HasNextPage.Should().BeFalse();
            pagedResult.TotalPages.Should().Be(1);
            pagedResult.TotalRecords.Should().Be(1);
            pagedResult.Results.Should().HaveCount(1);

            var salesPerson01 = pagedResult.Results.FirstOrDefault(x => x.Id == 274);
            salesPerson01!.Should().NotBeNull();
            salesPerson01!.FirstName.Should().Be("Stephen");
            salesPerson01!.LastName.Should().Be("Jiang");
        }
    }

    #region Private Helper Methods

    private static List<SalesPersonEntity> GetMockSalesPersonEntities()
    {
        var modifiedDate = new DateTime(2023, 1, 1);

        return new List<SalesPersonEntity>
        {
            new()
            {
                BusinessEntityId = 274,
                TerritoryId = 2,
                SalesQuota = 300000m,
                Bonus = 0m,
                CommissionPct = 0.012m,
                SalesYtd = 559697.5639m,
                SalesLastYear = 0m,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate,
                Employee = new EmployeeEntity
                {
                    BusinessEntityId = 274,
                    JobTitle = "North American Sales Manager",
                    ModifiedDate = modifiedDate,
                    PersonBusinessEntity = new PersonEntity
                    {
                        BusinessEntityId = 274,
                        FirstName = "Stephen",
                        LastName = "Jiang",
                        ModifiedDate = modifiedDate,
                        EmailAddresses = new List<EmailAddressEntity>
                        {
                            new()
                            {
                                BusinessEntityId = 274,
                                EmailAddressId = 1,
                                EmailAddressName = "stephen.jiang@adventure-works.com",
                                Rowguid = Guid.NewGuid(),
                                ModifiedDate = modifiedDate
                            }
                        }
                    }
                },
                SalesTerritory = new SalesTerritoryEntity
                {
                    TerritoryId = 2,
                    Name = "Northeast",
                    CountryRegionCode = "US",
                    Group = "North America",
                    ModifiedDate = modifiedDate
                }
            },
            new()
            {
                BusinessEntityId = 275,
                TerritoryId = 3,
                SalesQuota = 250000m,
                Bonus = 4100m,
                CommissionPct = 0.012m,
                SalesYtd = 3763178.1787m,
                SalesLastYear = 1750406.4785m,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate,
                Employee = new EmployeeEntity
                {
                    BusinessEntityId = 275,
                    JobTitle = "Sales Representative",
                    ModifiedDate = modifiedDate,
                    PersonBusinessEntity = new PersonEntity
                    {
                        BusinessEntityId = 275,
                        FirstName = "Michael",
                        LastName = "Blythe",
                        ModifiedDate = modifiedDate,
                        EmailAddresses = new List<EmailAddressEntity>
                        {
                            new()
                            {
                                BusinessEntityId = 275,
                                EmailAddressId = 1,
                                EmailAddressName = "michael.blythe@adventure-works.com",
                                Rowguid = Guid.NewGuid(),
                                ModifiedDate = modifiedDate
                            }
                        }
                    }
                },
                SalesTerritory = new SalesTerritoryEntity
                {
                    TerritoryId = 3,
                    Name = "Central",
                    CountryRegionCode = "US",
                    Group = "North America",
                    ModifiedDate = modifiedDate
                }
            },
            new()
            {
                BusinessEntityId = 276,
                TerritoryId = 4,
                SalesQuota = 250000m,
                Bonus = 2000m,
                CommissionPct = 0.012m,
                SalesYtd = 4251368.5497m,
                SalesLastYear = 1439156.0291m,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = modifiedDate,
                Employee = new EmployeeEntity
                {
                    BusinessEntityId = 276,
                    JobTitle = "Sales Representative",
                    ModifiedDate = modifiedDate,
                    PersonBusinessEntity = new PersonEntity
                    {
                        BusinessEntityId = 276,
                        FirstName = "Linda",
                        LastName = "Mitchell",
                        ModifiedDate = modifiedDate,
                        EmailAddresses = new List<EmailAddressEntity>
                        {
                            new()
                            {
                                BusinessEntityId = 276,
                                EmailAddressId = 1,
                                EmailAddressName = "linda.mitchell@adventure-works.com",
                                Rowguid = Guid.NewGuid(),
                                ModifiedDate = modifiedDate
                            }
                        }
                    }
                },
                SalesTerritory = new SalesTerritoryEntity
                {
                    TerritoryId = 4,
                    Name = "Southwest",
                    CountryRegionCode = "US",
                    Group = "North America",
                    ModifiedDate = modifiedDate
                }
            }
        };
    }

    #endregion
}
