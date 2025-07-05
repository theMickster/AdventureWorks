using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using AdventureWorks.Infrastructure.Persistence.Repositories.Person;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Person;

public sealed class PersonRepositoryTests : UnitTestBase
{
    private AdventureWorksDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AdventureWorksDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AdventureWorksDbContext(options);
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_ReturnsEntity_WhenValidEntraUser()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext();

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
            PersonTypeDescription = "Employee"
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = 1,
            PersonTypeId = 1,
            FirstName = "John",
            LastName = "Doe",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        context.BusinessEntities.Add(businessEntity);
        context.PersonTypes.Add(personType);
        context.Persons.Add(personEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(entraObjectId, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull("because a valid Entra-linked person exists");
            result!.BusinessEntityId.Should().Be(1, "because the BusinessEntityId should match");
            result.FirstName.Should().Be("John", "because the FirstName should match");
            result.LastName.Should().Be("Doe", "because the LastName should match");
        }
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_IncludesPersonType_InResult()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext();

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
            PersonTypeDescription = "Individual Customer"
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
            ModifiedDate = DefaultAuditDate
        };

        context.BusinessEntities.Add(businessEntity);
        context.PersonTypes.Add(personType);
        context.Persons.Add(personEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(entraObjectId, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the entity exists");
            result!.PersonType.Should().NotBeNull("because PersonType should be included");
            result.PersonType.PersonTypeName.Should().Be("Individual Customer", "because PersonType should be loaded");
        }
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_IncludesBusinessEntity_InResult()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext();

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
            PersonTypeDescription = "Employee"
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
            ModifiedDate = DefaultAuditDate
        };

        context.BusinessEntities.Add(businessEntity);
        context.PersonTypes.Add(personType);
        context.Persons.Add(personEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(entraObjectId, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull("because the entity exists");
            result!.BusinessEntity.Should().NotBeNull("because BusinessEntity should be included");
            result.BusinessEntity.IsEntraUser.Should().BeTrue("because IsEntraUser should be true");
            result.BusinessEntity.Rowguid.Should().Be(entraObjectId, "because Rowguid should match");
        }
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_ReturnsNull_WhenEntraObjectIdNotFound()
    {
        // Arrange
        var existingObjectId = Guid.NewGuid();
        var searchObjectId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext();

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 4,
            Rowguid = existingObjectId,
            IsEntraUser = true,
            ModifiedDate = DefaultAuditDate
        };

        var personType = new PersonTypeEntity
        {
            PersonTypeId = 4,
            PersonTypeCode = "EM",
            PersonTypeName = "Employee",
            PersonTypeDescription = "Employee"
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = 4,
            PersonTypeId = 4,
            FirstName = "Alice",
            LastName = "Williams",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        context.BusinessEntities.Add(businessEntity);
        context.PersonTypes.Add(personType);
        context.Persons.Add(personEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(searchObjectId, CancellationToken.None);

        // Assert
        result.Should().BeNull("because the EntraObjectId does not exist");
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_ReturnsNull_WhenIsEntraUserIsFalse()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext();

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 5,
            Rowguid = entraObjectId,
            IsEntraUser = false, // Not an Entra user
            ModifiedDate = DefaultAuditDate
        };

        var personType = new PersonTypeEntity
        {
            PersonTypeId = 5,
            PersonTypeCode = "IC",
            PersonTypeName = "Individual Customer",
            PersonTypeDescription = "Individual Customer"
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = 5,
            PersonTypeId = 5,
            FirstName = "Charlie",
            LastName = "Brown",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        context.BusinessEntities.Add(businessEntity);
        context.PersonTypes.Add(personType);
        context.Persons.Add(personEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(entraObjectId, CancellationToken.None);

        // Assert
        result.Should().BeNull("because IsEntraUser is false");
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_ReturnsNull_WhenBusinessEntityExistsButPersonDoesNot()
    {
        // Arrange
        var entraObjectId = Guid.NewGuid();
        await using var context = CreateInMemoryDbContext();

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 6,
            Rowguid = entraObjectId,
            IsEntraUser = true,
            ModifiedDate = DefaultAuditDate
        };

        // Add BusinessEntity but NOT Person
        context.BusinessEntities.Add(businessEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(entraObjectId, CancellationToken.None);

        // Assert
        result.Should().BeNull("because no Person record exists for this BusinessEntity");
    }

    [Fact]
    public async Task GetEntraLinkedPersonAsync_RespectsCancel()
    {
        // Arrange
        await using var context = CreateInMemoryDbContext();
        var repository = new PersonRepository(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Func<Task> act = async () => await repository.GetEntraLinkedPersonAsync(Guid.NewGuid(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>("because cancellation was requested");
    }

    [Theory]
    [InlineData("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]
    [InlineData("b2c3d4e5-f6a7-8901-bcde-f12345678901")]
    [InlineData("c3d4e5f6-a7b8-9012-cdef-123456789012")]
    public async Task GetEntraLinkedPersonAsync_WorksWithVariousGuids(string guidString)
    {
        // Arrange
        var entraObjectId = Guid.Parse(guidString);
        await using var context = CreateInMemoryDbContext();

        var businessEntity = new BusinessEntity
        {
            BusinessEntityId = 100 + guidString.GetHashCode() % 1000,
            Rowguid = entraObjectId,
            IsEntraUser = true,
            ModifiedDate = DefaultAuditDate
        };

        var personType = new PersonTypeEntity
        {
            PersonTypeId = 100 + guidString.GetHashCode() % 1000,
            PersonTypeCode = "EM",
            PersonTypeName = "Employee",
            PersonTypeDescription = "Employee"
        };

        var personEntity = new PersonEntity
        {
            BusinessEntityId = businessEntity.BusinessEntityId,
            PersonTypeId = personType.PersonTypeId,
            FirstName = "Test",
            LastName = "User",
            NameStyle = false,
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DefaultAuditDate
        };

        context.BusinessEntities.Add(businessEntity);
        context.PersonTypes.Add(personType);
        context.Persons.Add(personEntity);
        await context.SaveChangesAsync();

        var repository = new PersonRepository(context);

        // Act
        var result = await repository.GetEntraLinkedPersonAsync(entraObjectId, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull("because a valid entity exists for this GUID");
            result!.BusinessEntity.Rowguid.Should().Be(entraObjectId, "because Rowguid should match the search GUID");
        }
    }
}
