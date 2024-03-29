﻿using AdventureWorks.Domain.Entities;
using AdventureWorks.Infrastructure.Persistence.Repositories.Person;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Person;

[ExcludeFromCodeCoverage]
public sealed class BusinessEntityRepositoryTests : PersistenceUnitTestBase
{
    private readonly BusinessEntityRepository _sut;

    public BusinessEntityRepositoryTests()
    {
        _sut = new BusinessEntityRepository(DbContext);
        LoadMockPeople();
    }
    
    [Fact]
    public async Task Create_Read_Update_Delete_workflow_succeedsAsync()
    {
        var newEntity = new BusinessEntity { BusinessEntityId = -15, Rowguid = new Guid("489eed0a-6ca5-4dc2-9fc9-215a04c375b1"), ModifiedDate = StandardCreatedDate };

        var createResult = await _sut.AddAsync(newEntity);

        using (new AssertionScope())
        {
            createResult!.Should().NotBeNull();
            createResult.BusinessEntityId.Should().Be(newEntity.BusinessEntityId);
            createResult.ModifiedDate.Should().Be(StandardCreatedDate);
        }

        var updatedEntity = await _sut.GetByIdAsync(newEntity.BusinessEntityId);
        updatedEntity.ModifiedDate = StandardModifiedDate;

        await _sut.UpdateAsync(updatedEntity);

        var getResult = await _sut.GetByIdAsync(updatedEntity.BusinessEntityId);
        
        using (new AssertionScope())
        {
            getResult!.Should().NotBeNull();
            getResult.BusinessEntityId.Should().Be(newEntity.BusinessEntityId);
            getResult.ModifiedDate.Should().Be(StandardModifiedDate);
        }

        await _sut.DeleteAsync(getResult);

        var deleteResult = await _sut.GetByIdAsync(newEntity.BusinessEntityId);

        deleteResult?.Should().BeNull("because the entity should have been deleted");
    }
}
