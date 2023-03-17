using AdventureWorks.Application.Interfaces.Repositories.Sales;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.Repositories.Sales;

namespace AdventureWorks.UnitTests.Persistence.Repositories.Sales;

[ExcludeFromCodeCoverage]
public sealed class StoreRepositoryTests : PersistenceUnitTestBase
{
    private readonly StoreRepository _sut;

    public StoreRepositoryTests()
    {
        _sut = new StoreRepository(DbContext);
        LoadMockStores();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(StoreRepository)
                .Should().Implement<IStoreRepository>();

            typeof(StoreRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetStoreById_single_address_succeeds()
    {
        const int storeId = 1112;

        var result = await _sut.GetStoreById(storeId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result!.Should().NotBeNull();
            result.BusinessEntityId.Should().Be(storeId);

            result.StoreBusinessEntity!.Should().NotBeNull();
            result.StoreBusinessEntity.BusinessEntityId.Should().Be(storeId);

            result.StoreBusinessEntity.BusinessEntityAddresses!.Should().NotBeNullOrEmpty();
            result.StoreBusinessEntity.BusinessEntityAddresses.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task GetStoreById_multiple_addresses_succeeds()
    {
        const int storeId = 1111;
        var result = await _sut.GetStoreById(storeId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            result!.Should().NotBeNull();
            result.BusinessEntityId.Should().Be(storeId);

            result.StoreBusinessEntity!.Should().NotBeNull();
            result.StoreBusinessEntity.BusinessEntityId.Should().Be(storeId);

            result.StoreBusinessEntity.BusinessEntityAddresses!.Should().NotBeNullOrEmpty();
            result.StoreBusinessEntity.BusinessEntityAddresses.Should().HaveCount(3);
        }
    }

    [Fact]
    public async Task Create_Read_Update_Delete_workflow_succeeds()
    {
        var newEntity = new StoreEntity
        {
            BusinessEntityId = -12345, 
            Name = "Create_Read_Update_Delete_workflow Test",
            ModifiedDate = new DateTime(2011, 11, 11), 
            Rowguid = new Guid("acbd421e-67ab-49af-92fb-84b4d69364de"),
            SalesPersonId = -654,
            Demographics = "<root><hello>-856154035</hello></root>"
        };

        var createResult = await _sut.AddAsync(newEntity).ConfigureAwait(false);

        var getResult = await _sut.GetByIdAsync(newEntity.BusinessEntityId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            createResult.Should().NotBeNull();
            getResult.Should().NotBeNull();
            getResult.Customers?.Should().BeNull();
            getResult.PrimarySalesPerson?.Should().BeNull();
            getResult.StoreBusinessEntity?.Should().BeNull();
            createResult.BusinessEntityId.Should().Be(getResult.BusinessEntityId);
            getResult.Demographics.Should().Be(newEntity.Demographics);
        }

        getResult.Demographics = "<root><goodBye>-856154035</goodBye></root>";
        getResult.ModifiedDate = new DateTime(2023, 01, 01);

        await _sut.UpdateAsync(getResult).ConfigureAwait(false);

        var postUpdateEntity = await _sut.GetByIdAsync(newEntity.BusinessEntityId).ConfigureAwait(false);

        using (new AssertionScope())
        {
            postUpdateEntity.Should().NotBeNull();
            postUpdateEntity.ModifiedDate.Year.Should().Be(2023);
            postUpdateEntity.Demographics.Should().Contain("<goodBye>-856154035</goodBye>");
        }

        await _sut.DeleteAsync(postUpdateEntity).ConfigureAwait(false);

        var deleteResult = await _sut.GetByIdAsync(newEntity.BusinessEntityId).ConfigureAwait(false);
        
        deleteResult?.Should().BeNull("because the entity should have been deleted");
    }
}
