using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.Features.Sales.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.UnitTests.Setup.Fixtures;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadStoreContactListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IBusinessEntityContactEntityRepository> _mockBeceRepository = new();
    private ReadStoreContactListQueryHandler _sut;

    public ReadStoreContactListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(StoreEntityToModelProfile).Assembly)
        );
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadStoreContactListQueryHandler(_mapper, _mockBeceRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadStoreContactListQueryHandler(
                    null!,
                    _mockBeceRepository.Object)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadStoreContactListQueryHandler(
                    _mapper,
                    null!)))
                .Should().Throw<ArgumentNullException>("because we expect a null argument exception.")
                .And.ParamName.Should().Be("beceRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_nullAsync()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_no_contacts_foundAsync()
    {
        _mockBeceRepository.Setup(x => x.GetContactsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<BusinessEntityContactEntity>());

        var result = await _sut.Handle(new ReadStoreContactListQuery { StoreId = 9999 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_returns_mapped_contact_listAsync()
    {
        const int storeId = 2534;

        var contacts = SalesDomainFixtures.GetMockContactEntities()
            .Where(x => x.BusinessEntityId == storeId).ToList();

        _mockBeceRepository.Setup(x => x.GetContactsByIdAsync(storeId))
            .ReturnsAsync(contacts);

        var result = await _sut.Handle(new ReadStoreContactListQuery { StoreId = storeId }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Count(x => x.FirstName == "Steve").Should().Be(1);
            result.Count(x => x.FirstName == "Peter").Should().Be(1);
        }
    }

    [Fact]
    public async Task Handle_maps_StoreId_correctly_on_contactsAsync()
    {
        const int storeId = 2534;

        var contacts = SalesDomainFixtures.GetMockContactEntities()
            .Where(x => x.BusinessEntityId == storeId).ToList();

        _mockBeceRepository.Setup(x => x.GetContactsByIdAsync(storeId))
            .ReturnsAsync(contacts);

        var result = await _sut.Handle(new ReadStoreContactListQuery { StoreId = storeId }, CancellationToken.None);

        result.Should().AllSatisfy(c => c.StoreId.Should().Be(storeId));
    }
}
