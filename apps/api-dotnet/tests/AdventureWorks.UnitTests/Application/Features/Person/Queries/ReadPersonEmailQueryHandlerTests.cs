using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonEmailQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonEmailRepository> _mockRepo = new();
    private ReadPersonEmailQueryHandler _sut;

    public ReadPersonEmailQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(EmailAddressEntityToPersonEmailModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPersonEmailQueryHandler(_mapper, _mockRepo.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPersonEmailQueryHandler(null!, _mockRepo.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPersonEmailQueryHandler(_mapper, null!)))
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
    public async Task Handle_returns_null_when_email_not_found()
    {
        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmailAddressEntity?)null);

        var result = await _sut.Handle(
            new ReadPersonEmailQuery { PersonId = 1, EmailAddressId = 99 },
            CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_mapped_model_when_email_found()
    {
        var entity = new EmailAddressEntity
        {
            BusinessEntityId = 1,
            EmailAddressId = 2,
            EmailAddressName = "found@example.com",
            ModifiedDate = DefaultAuditDate
        };

        _mockRepo.Setup(x => x.GetEmailByCompositeKeyAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.Handle(
            new ReadPersonEmailQuery { PersonId = 1, EmailAddressId = 2 },
            CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.EmailAddressId.Should().Be(2);
            result.EmailAddress.Should().Be("found@example.com");
            result.ModifiedDate.Should().Be(DefaultAuditDate);
        }
    }
}
