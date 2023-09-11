using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Domain.Entities.Shield;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Application.Interfaces.DbContext;

public interface IAdventureWorksDbContext
{
    DbSet<Product> Products { get; set; }

    DbSet<AddressEntity> Addresses { get; set; }

    DbSet<AddressTypeEntity> AddressTypes { get; set; }

    DbSet<BusinessEntity> BusinessEntities { get; set; }

    DbSet<BusinessEntityContactEntity> BusinessEntityContacts { get; set; }

    DbSet<BusinessEntityAddressEntity> BusinessEntityAddresses { get; set; }

    DbSet<CountryRegionEntity> CountryRegions { get; set; }

    DbSet<EmailAddress> EmailAddresses { get; set; }

    DbSet<PersonEntity> Persons { get; set; }

    DbSet<SalesPerson> SalesPersons { get; set; }

    DbSet<SalesTerritoryEntity> SalesTerritories { get; set; }
    
    DbSet<SecurityFunctionEntity> SecurityFunctions { get; set; }
    
    DbSet<SecurityGroupEntity> SecurityGroups { get; set; }
    
    DbSet<SecurityGroupSecurityFunctionEntity> SecurityGroupSecurityFunctions { get; set; }
    
    DbSet<SecurityGroupSecurityRoleEntity> SecurityGroupSecurityRoles { get; set; }
    
    DbSet<SecurityGroupUserAccountEntity> SecurityGroupUserAccounts { get; set; }
    
    DbSet<SecurityRoleEntity> SecurityRoles { get; set; }

    DbSet<StateProvinceEntity> StateProvinces { get; set; }
    
    DbSet<UserAccountEntity> UserAccounts { get; set; }

    DbSet<UserRefreshTokenEntity> UserRefreshTokens { get; set; }

    DbSet<StoreEntity> Stores { get; set; }
}