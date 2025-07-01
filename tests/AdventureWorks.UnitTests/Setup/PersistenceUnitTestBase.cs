using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AdventureWorks.UnitTests.Setup;

[ExcludeFromCodeCoverage]
[SuppressMessage("Simplification", "CLASS0001:Seal class", Justification = "Not valid here...")]
public abstract class PersistenceUnitTestBase : UnitTestBase
{
    protected AdventureWorksDbContext DbContext;
    protected const int StandardCreatedBy = -101;
    protected const int StandardModifiedBy = -107;
    protected readonly DateTime StandardCreatedDate = new(2011, 11, 11, 11, 11, 11, DateTimeKind.Utc);
    protected readonly DateTime StandardModifiedDate = new(2011, 11, 11, 11, 11, 11, DateTimeKind.Utc);

    protected PersistenceUnitTestBase()
    {
        var options = new DbContextOptionsBuilder<AdventureWorksDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        DbContext = new AdventureWorksDbContext(options);

        DbContext.Database.EnsureCreated();

        DbContext.SaveChanges();

        Setup();
    }

    protected sealed override void Setup()
    {

    }
    
    protected void LoadMockPeople()
    {
        DbContext.BusinessEntities.AddRange(new List<BusinessEntity>
        {
            new(){BusinessEntityId = 1, Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 2, Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 3, Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 4, Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 5, Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 6, Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 7, Rowguid = new Guid("671d24b5-32d7-4ed5-8d37-e9380f1209b1"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 8, Rowguid = new Guid("9a22ce11-322f-477d-b62e-296d2f8794e0"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 9, Rowguid = new Guid("c379c89c-3000-49a6-be25-10ce21a1db62"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 10, Rowguid = new Guid("4a861fc4-409e-48f2-a211-5d6cd9833ed0"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 11, Rowguid = new Guid("ee0e2b2a-48d2-4d5f-863e-ab3bb5ad3547"),ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 12, Rowguid = new Guid("207a0b49-4e9b-4868-9831-7083399f1fd5"),ModifiedDate = StandardModifiedDate}
        });

        DbContext.Persons.AddRange(new List<PersonEntity>
        {
            new (){BusinessEntityId = 1, FirstName = "John", LastName = "Elway", Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 2, FirstName = "Terrell", LastName = "Davis", Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 3, FirstName = "Shannon", LastName = "Sharpe", Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 4, FirstName = "Emmitt", LastName = "Smith", Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 5, FirstName = "Duplicate", LastName = "User", Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 6, FirstName = "Duplicate", LastName = "User", Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 7, FirstName = "Rod", LastName = "Smith", Rowguid = new Guid("671d24b5-32d7-4ed5-8d37-e9380f1209b1"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 8, FirstName = "Randy", LastName = "Gradishar", Rowguid = new Guid("9a22ce11-322f-477d-b62e-296d2f8794e0"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 9, FirstName = "Floyd", LastName = "Little", Rowguid = new Guid("c379c89c-3000-49a6-be25-10ce21a1db62"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 10, FirstName = "Steve", LastName = "Atwater", Rowguid = new Guid("4a861fc4-409e-48f2-a211-5d6cd9833ed0"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 11, FirstName = "Ed", LastName = "McCaffrey", Rowguid = new Guid("ee0e2b2a-48d2-4d5f-863e-ab3bb5ad3547"), ModifiedDate = StandardModifiedDate},
            new (){BusinessEntityId = 12, FirstName = "Bill", LastName = "Romanowski", Rowguid = new Guid("207a0b49-4e9b-4868-9831-7083399f1fd5"), ModifiedDate = StandardModifiedDate}
        });
        
        DbContext.EmailAddresses.AddRange(new List<EmailAddressEntity>
        {
            new(){BusinessEntityId = 1, EmailAddressId = 1, EmailAddressName = "john.elway@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 2, EmailAddressId = 2, EmailAddressName = "terrell.davis@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 3, EmailAddressId = 3, EmailAddressName = "shannon.sharpe@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 4, EmailAddressId = 4, EmailAddressName = "emmitt.smith@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 5, EmailAddressId = 5, EmailAddressName = "Duplicate.User@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 6, EmailAddressId = 6, EmailAddressName = "Duplicate.User@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 7, EmailAddressId = 7, EmailAddressName = "rod.smith@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 8, EmailAddressId = 8, EmailAddressName = "randy.gradishar@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 9, EmailAddressId = 9, EmailAddressName = "floyd.little@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 10, EmailAddressId = 10, EmailAddressName = "steve.atwater@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 11, EmailAddressId = 11, EmailAddressName = "ed.mccaffrey@adventure-works.com",ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 12, EmailAddressId = 12, EmailAddressName = "bill.romanowski@adventure-works.com",ModifiedDate = StandardModifiedDate},
        });

        DbContext.SaveChanges();
    }

    protected void LoadMockStores()
    {
        var usa = new CountryRegionEntity { CountryRegionCode = "US",Name = "United States of America",ModifiedDate = StandardModifiedDate };
        var colorado = new StateProvinceEntity {StateProvinceId = 10,Name = "Colorado",CountryRegionCode = "US",CountryRegion = usa};

        DbContext.AddressTypes.AddRange(new List<AddressTypeEntity>
        {
            new() { AddressTypeId = 1, Name = "Home" },
            new() { AddressTypeId = 2, Name = "Billing" },
            new() { AddressTypeId = 3, Name = "Main Office" },
            new() { AddressTypeId = 4, Name = "Primary" },
            new() { AddressTypeId = 5, Name = "Secondary" },
            new() { AddressTypeId = 6, Name = "Tertiary" },
            new() { AddressTypeId = 7, Name = "Shipping" }
        });
        
        DbContext.BusinessEntities.AddRange(new List<BusinessEntity>
        {
            new(){BusinessEntityId = 1111, Rowguid = new Guid("d8f72edf-2334-4a59-abe7-a4f8cea37fb1"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1112, Rowguid = new Guid("f250a70a-39dd-4c7d-8031-51231e71b18f"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1113, Rowguid = new Guid("603782eb-457e-4824-9eee-781106958687"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1114, Rowguid = new Guid("454cd60c-ff39-4354-8b64-082e774785f8"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1115, Rowguid = new Guid("41cbd4ac-5c45-431e-999b-cae35ea49325"), ModifiedDate = StandardModifiedDate},
        });
        
        DbContext.Stores.AddRange(new List<StoreEntity>
        {
            new(){BusinessEntityId = 1111, Name = "Pro Sporting Goods", SalesPersonId = 7777, Rowguid = new Guid("a04ceec6-13da-49b7-8b1a-f3af8ba2c3b9"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1112, Name = "Topnotch Bikes", SalesPersonId = 7778, Rowguid = new Guid("93ca5ff6-7543-45c1-937c-5a06396d6f32"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1113, Name = "Golf and Cycle Store", SalesPersonId = 7779, Rowguid = new Guid("71272fd6-88ef-464a-9aaa-b3a6df4ad480"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1114, Name = "Colorado Ski, Golf, and Bike", SalesPersonId = 7780, Rowguid = new Guid("f28aabed-af5a-4bee-ae43-19ecd8995573"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1115, Name = "Epic Mountain Gear", SalesPersonId = 7781, Rowguid = new Guid("669b1923-c251-465e-bda5-f5378cb3961a"), ModifiedDate = StandardModifiedDate},

        });

        DbContext.SalesPersons.AddRange(new List<SalesPersonEntity>
        {
            new(){BusinessEntityId = 7777, TerritoryId = 4, Rowguid = new Guid("1e28194c-6f14-4dc9-b4ff-e715ebd606ef"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 7778, TerritoryId = 4, Rowguid = new Guid("2126a5df-a5ba-4261-878d-c85b9ea39aa9"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 7779, TerritoryId = 4, Rowguid = new Guid("9ef29acb-622e-4b58-9825-987bb22c5b23"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 7780, TerritoryId = 4, Rowguid = new Guid("3c11ef02-88d6-4303-8729-bff99da0c720"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 7781, TerritoryId = 4, Rowguid = new Guid("b9b61d15-e466-46c6-9795-6aa07457190e"), ModifiedDate = StandardModifiedDate},
        });

        DbContext.Addresses.AddRange( new List<AddressEntity>{
            new()
            {
                AddressId = 553,
                AddressLine1 = "1234 Broadway Ave",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("8f83c8eb-ee79-46ba-8e2c-6645794674b4"),
                PostalCode = "80015",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
            new()
            {
                AddressId = 554,
                AddressLine1 = "456 Union Ave",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("28238dcb-1842-4a64-9224-6b13cbbb0ab4"),
                PostalCode = "80016",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
            new()
            {
                AddressId = 555,
                AddressLine1 = "487 Wheeling Ave",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("6934f6ca-6382-48cc-98e7-4a1575c7c62c"),
                PostalCode = "80012",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
            new()
            {
                AddressId = 556,
                AddressLine1 = "942 Mississippi Ave",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("214d2137-4254-4a50-b8ef-7dae4893bd7b"),
                PostalCode = "80012",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
            new()
            {
                AddressId = 557,
                AddressLine1 = "154 Evans St.",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("da409df8-2b7a-465b-be4f-f489f8b87d41"),
                PostalCode = "80011",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
            new()
            {
                AddressId = 558,
                AddressLine1 = "348 St. Thomas Lane",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("54999039-5cbe-43b8-9e4a-8e6529a6a280"),
                PostalCode = "80010",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
            new()
            {
                AddressId = 559,
                AddressLine1 = "414 South Tejon Parkway",
                City = "Aurora",
                StateProvinceId = 10,
                Rowguid = new Guid("9a540808-7509-40bd-befb-e5f20f38af49"),
                PostalCode = "80010",
                ModifiedDate = StandardModifiedDate,
                StateProvince = colorado
            },
        });

        DbContext.BusinessEntityAddresses.AddRange(new List<BusinessEntityAddressEntity>
        {
            new(){BusinessEntityId = 1111, AddressId = 553, AddressTypeId = 4, Rowguid = new Guid("d8f72edf-2334-4a59-abe7-a4f8cea37fb1"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1111, AddressId = 554, AddressTypeId = 3, Rowguid = new Guid("c8679617-b372-4d53-803b-cced0afdf8f3"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1111, AddressId = 555, AddressTypeId = 5, Rowguid = new Guid("7a4aea59-2b12-41a3-893c-8469c04fc581"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1112, AddressId = 556, AddressTypeId = 4, Rowguid = new Guid("f250a70a-39dd-4c7d-8031-51231e71b18f"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1113, AddressId = 557, AddressTypeId = 3, Rowguid = new Guid("603782eb-457e-4824-9eee-781106958687"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1114, AddressId = 558, AddressTypeId = 2, Rowguid = new Guid("454cd60c-ff39-4354-8b64-082e774785f8"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1115, AddressId = 559, AddressTypeId = 3, Rowguid = new Guid("41cbd4ac-5c45-431e-999b-cae35ea49325"), ModifiedDate = StandardModifiedDate},
        });

        DbContext.SaveChanges();
    }

    protected void LoadMockSalesPersons()
    {
        var usa = new CountryRegionEntity { CountryRegionCode = "US", Name = "United States of America", ModifiedDate = StandardModifiedDate };
        var colorado = new StateProvinceEntity { StateProvinceId = 10, Name = "Colorado", CountryRegionCode = "US", CountryRegion = usa };

        DbContext.AddressTypes.AddRange(new List<AddressTypeEntity>
        {
            new() { AddressTypeId = 1, Name = "Home" },
            new() { AddressTypeId = 2, Name = "Billing" },
            new() { AddressTypeId = 3, Name = "Main Office" },
            new() { AddressTypeId = 4, Name = "Primary" },
            new() { AddressTypeId = 5, Name = "Secondary" },
            new() { AddressTypeId = 6, Name = "Tertiary" },
            new() { AddressTypeId = 7, Name = "Shipping" }
        });

        DbContext.SalesPersons.AddRange(new List<SalesPersonEntity>
        {
            new(){BusinessEntityId = 7777, TerritoryId = 4, Rowguid = new Guid("1e28194c-6f14-4dc9-b4ff-e715ebd606ef"), ModifiedDate = StandardModifiedDate},
        });

        DbContext.SalesTerritories.AddRange(new List<SalesTerritoryEntity>
        {
            new(){TerritoryId = 1 , Name = "Northwest", CountryRegionCode = "US",Group = "North America", SalesYtd = 7887186.79M, SalesLastYear = 3298694.49M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("43689A10-E30B-497F-B0DE-11DE20267FF7"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 2 , Name = "Northeast", CountryRegionCode = "US",Group = "North America", SalesYtd = 2402176.85M, SalesLastYear = 3607148.94M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("00FB7309-96CC-49E2-8363-0A1BA72486F2"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 3 , Name = "Central", CountryRegionCode = "US",Group = "North America", SalesYtd = 3072175.12M, SalesLastYear = 3205014.08M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("DF6E7FD8-1A8D-468C-B103-ED8ADDB452C1"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 4 , Name = "Southwest", CountryRegionCode = "US",Group = "North America", SalesYtd = 10510853.87M, SalesLastYear = 5366575.71M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("DC3E9EA0-7950-4431-9428-99DBCBC33865"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 5 , Name = "Southeast", CountryRegionCode = "US",Group = "North America", SalesYtd = 2538667.25M, SalesLastYear = 3925071.43M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("6DC4165A-5E4C-42D2-809D-4344E0AC75E7"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 6 , Name = "Canada", CountryRegionCode = "CA",Group = "North America", SalesYtd = 6771829.14M, SalesLastYear = 5693988.86M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("06B4AF8A-1639-476E-9266-110461D66B00"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 7 , Name = "France", CountryRegionCode = "FR",Group = "Europe", SalesYtd = 4772398.31M, SalesLastYear = 2396539.76M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("BF806804-9B4C-4B07-9D19-706F2E689552"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 8 , Name = "Germany", CountryRegionCode = "DE",Group = "Europe", SalesYtd = 3805202.35M, SalesLastYear = 1307949.79M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("6D2450DB-8159-414F-A917-E73EE91C38A9"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 9 , Name = "Australia", CountryRegionCode = "AU",Group = "Pacific", SalesYtd = 5977814.92M, SalesLastYear = 2278548.98M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("602E612E-DFE9-41D9-B894-27E489747885"), ModifiedDate = DefaultAuditDate},
            new(){TerritoryId = 10 , Name = "United Kingdom", CountryRegionCode = "GB",Group = "Europe", SalesYtd = 5012905.37M, SalesLastYear = 1635823.40M, CostYtd = 0.00M, CostLastYear = 0.00M, Rowguid = new Guid("05FC7E1F-2DEA-414E-9ECD-09D150516FB5"), ModifiedDate = DefaultAuditDate}
        });

        DbContext.SaveChanges();
    }

    protected void LoadMockBusinessEntityContacts()
    {
        DbContext.BusinessEntities.AddRange(new List<BusinessEntity>
        {
            new(){BusinessEntityId = 1111, Rowguid = new Guid("d8f72edf-2334-4a59-abe7-a4f8cea37fb1"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1112, Rowguid = new Guid("f250a70a-39dd-4c7d-8031-51231e71b18f"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1113, Rowguid = new Guid("f250a70a-39dd-4c7d-8031-51231e71b18f"), ModifiedDate = StandardModifiedDate},
            new(){BusinessEntityId = 1114, Rowguid = new Guid("f250a70a-39dd-4c7d-8031-51231e71b18f"), ModifiedDate = StandardModifiedDate}
        });

        DbContext.Persons.AddRange(new List<PersonEntity>
        {
            new (){BusinessEntityId = 1, FirstName = "John", LastName = "Elway", Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 2, FirstName = "Terrell", LastName = "Davis", Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"), ModifiedDate = StandardModifiedDate, PersonTypeId = 4},
            new (){BusinessEntityId = 3, FirstName = "Shannon", LastName = "Sharpe", Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e"), ModifiedDate = StandardModifiedDate, PersonTypeId = 3},
            new (){BusinessEntityId = 4, FirstName = "Emmitt", LastName = "Smith", Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"), ModifiedDate = StandardModifiedDate, PersonTypeId = 3},
            new (){BusinessEntityId = 5, FirstName = "Duplicate", LastName = "User", Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"), ModifiedDate = StandardModifiedDate, PersonTypeId = 3},
            new (){BusinessEntityId = 6, FirstName = "Duplicate", LastName = "User", Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 7, FirstName = "Rod", LastName = "Smith", Rowguid = new Guid("671d24b5-32d7-4ed5-8d37-e9380f1209b1"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 8, FirstName = "Randy", LastName = "Gradishar", Rowguid = new Guid("9a22ce11-322f-477d-b62e-296d2f8794e0"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 9, FirstName = "Floyd", LastName = "Little", Rowguid = new Guid("c379c89c-3000-49a6-be25-10ce21a1db62"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 10, FirstName = "Steve", LastName = "Atwater", Rowguid = new Guid("4a861fc4-409e-48f2-a211-5d6cd9833ed0"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 11, FirstName = "Ed", LastName = "McCaffrey", Rowguid = new Guid("ee0e2b2a-48d2-4d5f-863e-ab3bb5ad3547"), ModifiedDate = StandardModifiedDate, PersonTypeId = 1},
            new (){BusinessEntityId = 12, FirstName = "Bill", LastName = "Romanowski", Rowguid = new Guid("207a0b49-4e9b-4868-9831-7083399f1fd5"), ModifiedDate = StandardModifiedDate, PersonTypeId = 4}
        });

        DbContext.ContactTypes.AddRange(new List<ContactTypeEntity>
        {
            new(){ContactTypeId = 1, Name = "Accounting Manager"},
            new(){ContactTypeId = 11, Name = "Owner"},
            new(){ContactTypeId = 13, Name = "Product Manager"},
            new(){ContactTypeId = 14, Name = "Purchasing Agent"},
            new(){ContactTypeId = 15, Name = "Purchasing Manager"},
            new(){ContactTypeId = 17, Name = "Sales Agent"},
            new(){ContactTypeId = 18, Name = "Sales Associate"},
            new(){ContactTypeId = 19, Name = "Sales Manager"}
        });

        DbContext.PersonTypes.AddRange(new List<PersonTypeEntity>
        {
            new(){PersonTypeId = 1, PersonTypeName = "Store Contact", PersonTypeCode = "SC"},
            new(){PersonTypeId = 3, PersonTypeName = "Sales Person", PersonTypeCode = "SP"},
            new(){PersonTypeId = 4, PersonTypeName = "Employee (Non-Sales Person)", PersonTypeCode = "EM"},
        });

        DbContext.BusinessEntityContacts.AddRange(new List<BusinessEntityContactEntity>
        {
            new(){BusinessEntityId = 1112, ContactTypeId = 11, PersonId = 12},
            new(){BusinessEntityId = 1111, ContactTypeId = 1, PersonId = 1},
            new(){BusinessEntityId = 1111, ContactTypeId = 11, PersonId = 2},
            new(){BusinessEntityId = 1111, ContactTypeId = 17, PersonId = 3},
            new(){BusinessEntityId = 1111, ContactTypeId = 18, PersonId = 4},
            new(){BusinessEntityId = 1111, ContactTypeId = 19, PersonId = 7},
            new(){BusinessEntityId = 1113, ContactTypeId = 1, PersonId = 8},
            new(){BusinessEntityId = 1113, ContactTypeId = 11, PersonId = 9},
            new(){BusinessEntityId = 1114, ContactTypeId = 1, PersonId = 10},
            new(){BusinessEntityId = 1114, ContactTypeId = 11, PersonId = 11}
        });

        DbContext.SaveChanges();
    }
}

