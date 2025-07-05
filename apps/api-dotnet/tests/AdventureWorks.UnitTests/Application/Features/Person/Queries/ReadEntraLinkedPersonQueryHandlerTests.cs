using AdventureWorks.Application.Features.Person.Profiles;
using AdventureWorks.Application.Features.Person.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories.Person;
using AdventureWorks.Domain.Entities.Person;
using Microsoft.Extensions.Logging;

namespace AdventureWorks.UnitTests.Application.Features.Person.Queries;

public sealed class ReadEntraLinkedPersonQueryHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IPersonRepository> _mockPersonRepository = new();
    private readonly Mock<ILogger<ReadEntraLinkedPersonQueryHandler>> _mockLogger = new();
    private ReadEntraLinkedPersonQueryHandler _sut;

    public ReadEntraLinkedPersonQueryHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(config =>
            config.AddMaps(typeof(EntraLinkedPersonProfile).Assembly));

        _mapper = mappingConfig.CreateMapper();
        _sut = new ReadEntraLinkedPersonQueryHandler(_mapper, _mockPersonRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
    {
        // Act
        var act = () => _sut = new ReadEntraLinkedPersonQueryHandler(
            null!, 
            _mockPersonRepository.Object, 
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("mapper");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
    {
        // Act
        var act = () => _sut = new ReadEntraLinkedPersonQueryHandler(
            _mapper, 
            null!, 
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("personRepository");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        // Act
        var act = () => _sut = new ReadEntraLinkedPersonQueryHandler(
            _mapper, 
            _mockPersonRepository.Object, 
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public async Task Handle_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        // Act
        var act = async () => await _sut.Handle(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenPersonNotFound()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonEntity?)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull("because the person was not found");
    }

    [Fact]
    public async Task Handle_LogsWarning_WhenPersonNotFound()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonEntity?)null);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found or not linked")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because a warning should be logged when person is not found");
    }

    [Fact]
    public async Task Handle_ReturnsValidModel_WhenPersonExists()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 1,
            Rowguid = entraObjectId,
            IsEntraUser = true,
            ModifiedDate = DefaultAuditDate
        };

        var personType = new PersonTypeEntity
        {
            PersonTypeId = 1,
            PersonTypeCode = "EM",
            PersonTypeName = "Employee",
            PersonTypeDescription = "Employee",
            CreatedOn = DefaultAuditDate,
            ModifiedOn = DefaultAuditDate
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = 1,
            PersonTypeId = 1,
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "A",
            Title = "Mr.",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate,
            BusinessEntity = businessEntity,
            PersonType = personType,
            EmailAddresses = new List<EmailAddressEntity>
            {
                new()
                {
                    BusinessEntityId = 1,
                    EmailAddressId = 1,
                    EmailAddressName = "john.doe@adventure-works.com",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DefaultAuditDate
                }
            }
        };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personEntity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the person exists");
            result!.BusinessEntityId.Should().Be(1, "because BusinessEntityId should map correctly");
            result.EntraObjectId.Should().Be(entraObjectId, "because EntraObjectId should map from BusinessEntity.Rowguid");
            result.FirstName.Should().Be("John", "because FirstName should map correctly");
            result.LastName.Should().Be("Doe", "because LastName should map correctly");
            result.MiddleName.Should().Be("A", "because MiddleName should map correctly");
            result.Title.Should().Be("Mr.", "because Title should map correctly");
            result.EmailAddress.Should().Be("john.doe@adventure-works.com", "because EmailAddress should resolve from collection");
            result.PersonTypeId.Should().Be(1, "because PersonTypeId should map correctly");
            result.PersonTypeName.Should().Be("Employee", "because PersonTypeName should map from navigation property");
            result.IsEntraUser.Should().BeTrue("because IsEntraUser should map from BusinessEntity");
        }
    }

    [Fact]
    public async Task Handle_MapsEmailAddress_FromFirstEmailInCollection()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 2,
            Rowguid = entraObjectId,
            IsEntraUser = true,
            ModifiedDate = DefaultAuditDate
        };

        var personType = new PersonTypeEntity
        {
            PersonTypeId = 2,
            PersonTypeCode = "IC",
            PersonTypeName = "Individual Customer",
            PersonTypeDescription = "Individual Customer",
            CreatedOn = DefaultAuditDate,
            ModifiedOn = DefaultAuditDate
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = 2,
            PersonTypeId = 2,
            FirstName = "Jane",
            LastName = "Smith",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate,
            BusinessEntity = businessEntity,
            PersonType = personType,
            EmailAddresses = new List<EmailAddressEntity>
            {
                new()
                {
                    BusinessEntityId = 2,
                    EmailAddressId = 1,
                    EmailAddressName = "jane.smith@adventure-works.com",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DefaultAuditDate
                },
                new()
                {
                    BusinessEntityId = 2,
                    EmailAddressId = 2,
                    EmailAddressName = "jane.smith.alt@adventure-works.com",
                    Rowguid = Guid.NewGuid(),
                    ModifiedDate = DefaultAuditDate
                }
            }
        };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personEntity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result!.EmailAddress.Should().Be("jane.smith@adventure-works.com", 
            "because the first email address should be mapped");
    }

    [Fact]
    public async Task Handle_HandlesNullEmailAddress_Gracefully()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 3,
            Rowguid = entraObjectId,
            IsEntraUser = true,
            ModifiedDate = DefaultAuditDate
        };

        var personType = new PersonTypeEntity
        {
            PersonTypeId = 3,
            PersonTypeCode = "EM",
            PersonTypeName = "Employee",
            PersonTypeDescription = "Employee",
            CreatedOn = DefaultAuditDate,
            ModifiedOn = DefaultAuditDate
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = 3,
            PersonTypeId = 3,
            FirstName = "Bob",
            LastName = "Johnson",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate,
            BusinessEntity = businessEntity,
            PersonType = personType,
            EmailAddresses = new List<EmailAddressEntity>() // Empty email collection
        };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personEntity);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result!.EmailAddress.Should().BeNull("because no email address exists in the collection");
    }

    [Fact]
    public async Task Handle_CallsRepository_WithCorrectEntraObjectId()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonEntity?)null);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _mockPersonRepository.Verify(
            x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()),
            Times.Once,
            "because repository should be called with the correct EntraObjectId");
    }

    [Theory]
    [InlineData("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
    [InlineData("b2c3d4e5-f6a7-8901-bcde-f12345678901")]
    [InlineData("c3d4e5f6-a7b8-9012-cdef-123456789012")]
    public async Task Handle_PassesEntraObjectId_ToRepository(string guidString)
    {
        // Arrange
        var entraObjectId = Guid.Parse(guidString);
        var query = new ReadEntraLinkedPersonQuery { EntraObjectId = entraObjectId };

        _mockPersonRepository
            .Setup(x => x.GetEntraLinkedPersonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonEntity?)null);

        // Act
        await _sut.Handle(query, CancellationToken.None);

        // Assert
        _mockPersonRepository.Verify(
            x => x.GetEntraLinkedPersonAsync(entraObjectId, It.IsAny<CancellationToken>()),
            Times.Once,
            $"because repository should be called with GUID {guidString}");
    }
}
