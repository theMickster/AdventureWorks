using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Application.PersistenceContracts.DbContext;

public interface IAdventureWorksDbContext
{
    DbSet<Product> Products { get; set; }

    DbSet<AddressEntity> Addresses { get; set; }

    DbSet<AddressTypeEntity> AddressTypes { get; set; }

    DbSet<BusinessEntity> BusinessEntities { get; set; }

    DbSet<BusinessEntityContactEntity> BusinessEntityContacts { get; set; }

    DbSet<BusinessEntityAddressEntity> BusinessEntityAddresses { get; set; }

    DbSet<CountryRegionEntity> CountryRegions { get; set; }

    DbSet<DepartmentEntity> Departments { get; set; }

    DbSet<EmailAddressEntity> EmailAddresses { get; set; }

    DbSet<EmployeeEntity> Employees { get; set; }

    DbSet<PersonEntity> Persons { get; set; }

    DbSet<PersonPhone> PersonPhones { get; set; }

    DbSet<PhoneNumberTypeEntity> PhoneNumberTypes { get; set; }

    DbSet<SalesPersonEntity> SalesPersons { get; set; }

    DbSet<ShiftEntity> Shifts { get; set; }

    DbSet<SalesTerritoryEntity> SalesTerritories { get; set; }
    
    DbSet<StateProvinceEntity> StateProvinces { get; set; }

    DbSet<StoreEntity> Stores { get; set; }
}