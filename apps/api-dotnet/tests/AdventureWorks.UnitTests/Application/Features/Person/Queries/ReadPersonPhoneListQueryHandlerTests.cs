using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonPhoneListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonPhoneRepository> _mockRepo = new();
    private ReadPersonPhoneListQueryHandler _sut;

    public ReadPersonPhoneListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(PersonPhoneEntityToPersonPhoneModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPersonPhoneListQueryHandler(_mapper, _mockRepo.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPersonPhoneListQueryHandler(null!, _mockRepo.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPersonPhoneListQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personPhoneRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_throws_KeyNotFoundException_when_person_does_not_exist()
    {
        _mockRepo.Setup(x => x.PersonExistsAsync(9999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await _sut.Handle(
            new ReadPersonPhoneListQuery { PersonId = 9999 },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_person_has_no_phones()
    {
        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.GetPhonesByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.Handle(new ReadPersonPhoneListQuery { PersonId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_returns_mapped_phone_list()
    {
        var cellType = new PhoneNumberTypeEntity { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = DefaultAuditDate };
        var homeType = new PhoneNumberTypeEntity { PhoneNumberTypeId = 2, Name = "Home", ModifiedDate = DefaultAuditDate };

        var phones = new List<PersonPhone>
        {
            new() { BusinessEntityId = 1, PhoneNumber = "555-1111", PhoneNumberTypeId = 1, ModifiedDate = DefaultAuditDate, PhoneNumberType = cellType },
            new() { BusinessEntityId = 1, PhoneNumber = "555-2222", PhoneNumberTypeId = 2, ModifiedDate = DefaultAuditDate, PhoneNumberType = homeType }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.GetPhonesByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(phones);

        var result = await _sut.Handle(new ReadPersonPhoneListQuery { PersonId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.PhoneNumber == "555-1111");
            result.Should().Contain(p => p.PhoneNumber == "555-2222");
        }
    }
}
