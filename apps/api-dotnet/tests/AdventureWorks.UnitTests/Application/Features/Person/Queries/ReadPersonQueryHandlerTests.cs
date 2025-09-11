using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadPersonQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonRepository> _mockRepo = new();
    private ReadPersonQueryHandler _sut;

    public ReadPersonQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(PersonEntityToPersonDetailModelProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();

        _sut = new ReadPersonQueryHandler(_mapper, _mockRepo.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        using (new AssertionScope())
        {
            _ = ((Action)(() => _sut = new ReadPersonQueryHandler(null!, _mockRepo.Object)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("mapper");

            _ = ((Action)(() => _sut = new ReadPersonQueryHandler(_mapper, null!)))
                .Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("personRepository");
        }
    }

    [Fact]
    public async Task Handle_throws_ArgumentNullException_when_request_is_null()
    {
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task Handle_returns_null_when_person_not_found()
    {
        _mockRepo.Setup(x => x.GetPersonDetailByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonEntity?)null);

        var result = await _sut.Handle(new ReadPersonQuery { PersonId = 1 }, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_returns_mapped_model_when_person_found()
    {
        var entity = new PersonEntity
        {
            BusinessEntityId = 1,
            PersonTypeId = 2,
            FirstName = "John",
            LastName = "Doe",
            NameStyle = false,
            EmailPromotion = 1,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate,
            PersonType = new PersonTypeEntity
            {
                PersonTypeId = 2,
                PersonTypeName = "Individual Customer",
                PersonTypeCode = "IC",
                PersonTypeDescription = "Customer"
            },
            EmailAddresses =
            [
                new EmailAddressEntity
                {
                    BusinessEntityId = 1,
                    EmailAddressId = 11,
                    EmailAddressName = "john.doe@example.com",
                    ModifiedDate = DefaultAuditDate
                }
            ],
            PersonPhones =
            [
                new PersonPhone
                {
                    BusinessEntityId = 1,
                    PhoneNumber = "555-0100",
                    PhoneNumberTypeId = 1,
                    PhoneNumberType = new PhoneNumberTypeEntity
                    {
                        PhoneNumberTypeId = 1,
                        Name = "Cell",
                        ModifiedDate = DefaultAuditDate
                    },
                    ModifiedDate = DefaultAuditDate
                }
            ]
        };

        _mockRepo.Setup(x => x.GetPersonDetailByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.Handle(new ReadPersonQuery { PersonId = 1 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(1);
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");
            result.PersonTypeName.Should().Be("Individual Customer");
            result.EmailAddresses.Should().ContainSingle(x => x.EmailAddressId == 11 && x.EmailAddress == "john.doe@example.com");
            result.PhoneNumbers.Should().ContainSingle(x => x.PhoneNumberTypeName == "Cell" && x.PhoneNumber == "555-0100");
        }
    }

    [Fact]
    public async Task Handle_returns_model_when_person_type_is_null()
    {
        var entity = new PersonEntity
        {
            BusinessEntityId = 2,
            PersonTypeId = 1,
            FirstName = "Null",
            LastName = "Type",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate,
            PersonType = null
        };

        _mockRepo.Setup(x => x.GetPersonDetailByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.Handle(new ReadPersonQuery { PersonId = 2 }, CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(2);
            result.PersonTypeName.Should().BeNull();
        }
    }
}
