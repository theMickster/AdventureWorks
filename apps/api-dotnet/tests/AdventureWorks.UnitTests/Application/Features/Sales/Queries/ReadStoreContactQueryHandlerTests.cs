using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreContactQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockBeceRepository = new();
    private ReadStoreContactQueryHandler _sut;

    public ReadStoreContactQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(BusinessEntityContactEntityToStoreContactModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreContactQueryHandler(_mapper, _mockBeceRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreContactQueryHandler(null!, _mockBeceRepository.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreContactQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("businessEntityContactRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_null_when_contact_not_foundAsync()
    {
        var query = new ReadStoreContactQuery { StoreId = 2534, PersonId = 100, ContactTypeId = 11 };

        _mockBeceRepository.Setup(x => x.GetWithDetailsByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessEntityContactEntity?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_mapped_modelAsync()
    {
        var query = new ReadStoreContactQuery { StoreId = 2534, PersonId = 100, ContactTypeId = 11 };

        var hydrated = new BusinessEntityContactEntity
        {
            BusinessEntityId = 2534,
            PersonId = 100,
            ContactTypeId = 11,
            ModifiedDate = DefaultAuditDate,
            ContactType = new ContactTypeEntity { ContactTypeId = 11, Name = "Owner" },
            Person = new PersonEntity
            {
                BusinessEntityId = 100,
                FirstName = "Pat",
                LastName = "Smith",
                MiddleName = string.Empty,
                Title = string.Empty,
                Suffix = string.Empty
            }
        };

        _mockBeceRepository.Setup(x => x.GetWithDetailsByCompositeKeyAsync(2534, 100, 11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hydrated);

        var result = await _sut.Handle(query, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.StoreId.Should().Be(2534);
            result!.Id.Should().Be(100);
            result!.ContactTypeId.Should().Be(11);
            result!.ContactTypeName.Should().Be("Owner");
            result!.FirstName.Should().Be("Pat");
            result!.LastName.Should().Be("Smith");
        }
    }
}
