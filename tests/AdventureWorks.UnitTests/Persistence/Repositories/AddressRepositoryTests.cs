using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories;

[ExcludeFromCodeCoverage]
public sealed class AddressRepositoryTests : PersistenceUnitTestBase
{
    private readonly AddressRepository _sut;

    public AddressRepositoryTests()
    {
        _sut = new AddressRepository(DbContext);

        DbContext.Addresses.Add(
            new AddressEntity
            {
                AddressId = 395,
                AddressLine1 = "1234 Colorado Ave",
                AddressLine2 = "Unit 302A",
                City = "Phoenix",
                StateProvinceId = 39,
                Rowguid = new Guid("19c5db67-b8ef-4b80-98a9-a2aa962de4e2"),
                PostalCode = "88751",
                ModifiedDate = new DateTime(2011, 11, 11),
                StateProvince = new StateProvinceEntity
                {
                    StateProvinceId = 39, 
                    Name = "Arizona", 
                    CountryRegionCode = "US",
                    CountryRegion = new CountryRegionEntity
                    {
                        CountryRegionCode = "US",
                        Name = "United States of America",
                        ModifiedDate = new DateTime(2011, 11, 11)
                    }
                }
            }
        );

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(AddressRepository)
                .Should().Implement<IAddressRepository>();

            typeof(AddressRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(395)]
    public async Task GetAddressByIdAsync_succeeds_Async(int addressId)
    {
        var result = await _sut.GetAddressByIdAsync(addressId);

        using (new AssertionScope())
        {
            result!.Should().NotBeNull();

            result.AddressId.Should().Be(395);

            result.StateProvince.Should().NotBeNull();

            result.StateProvince.CountryRegion.Should().NotBeNull();
        }
    }

    [Theory]
    [InlineData(395)]
    public async Task GetByIdAsync_succeeds_Async(int addressId)
    {
        var result = await _sut.GetByIdAsync(addressId);

        using (new AssertionScope())
        {
            result!.Should().NotBeNull();

            result.AddressId.Should().Be(395);

            result.StateProvince.Should().NotBeNull();

            result.StateProvince.CountryRegion.Should().NotBeNull();
        }
    }

}
