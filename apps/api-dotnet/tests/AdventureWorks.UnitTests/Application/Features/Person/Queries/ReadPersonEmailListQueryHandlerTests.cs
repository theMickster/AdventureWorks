using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonEmailListQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonEmailRepository> _mockRepo = new();
    private ReadPersonEmailListQueryHandler _sut;

    public ReadPersonEmailListQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(EmailAddressEntityToPersonEmailModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPersonEmailListQueryHandler(_mapper, _mockRepo.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPersonEmailListQueryHandler(null!, _mockRepo.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPersonEmailListQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personEmailRepository");
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
            new ReadPersonEmailListQuery { PersonId = 9999 },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_person_has_no_emails()
    {
        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.GetEmailsByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _sut.Handle(new ReadPersonEmailListQuery { PersonId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Handle_returns_mapped_email_list()
    {
        var emails = new List<EmailAddressEntity>
        {
            new() { BusinessEntityId = 1, EmailAddressId = 1, EmailAddressName = "a@example.com", ModifiedDate = DefaultAuditDate },
            new() { BusinessEntityId = 1, EmailAddressId = 2, EmailAddressName = "b@example.com", ModifiedDate = DefaultAuditDate }
        };

        _mockRepo.Setup(x => x.PersonExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockRepo.Setup(x => x.GetEmailsByPersonIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emails);

        var result = await _sut.Handle(new ReadPersonEmailListQuery { PersonId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().HaveCount(2);
            result.Should().Contain(e => e.EmailAddress == "a@example.com");
            result.Should().Contain(e => e.EmailAddress == "b@example.com");
        }
    }
}
