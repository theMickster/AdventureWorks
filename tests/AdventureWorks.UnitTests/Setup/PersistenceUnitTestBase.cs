using System.Security.Cryptography;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Shield;
using AdventureWorks.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Polly;

namespace AdventureWorks.UnitTests.Setup;

[ExcludeFromCodeCoverage]
[SuppressMessage("Simplification", "CLASS0001:Seal class", Justification = "Not valid here...")]
public abstract class PersistenceUnitTestBase : UnitTestBase
{
    protected AdventureWorksDbContext DbContext;
    private const int _standardCreatedBy = -101;
    private const int _standardModifiedBy = -107;
    private DateTime _standardCreatedDate = new DateTime(2011, 11, 11);
    private DateTime _standardModifiedDate = new DateTime(2021, 11, 11);

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

    protected void LoadMockUserSecurityData()
    {
        DbContext.UserAccounts.AddRange(new List<UserAccountEntity>
        {
            new()
            {
                BusinessEntityId = 1,
                UserName = "john.elway",
                RecordId = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 1,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 2,
                UserName = "terrell.davis",
                RecordId = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 2,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 3,
                UserName = "shannon.sharpe",
                RecordId = new Guid("94159810-21c3-4666-ba28-04911f05215e"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 3,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 4,
                UserName = "emmitt.smith",
                RecordId = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 4,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 5,
                UserName = "Duplicate.User",
                RecordId = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 5,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 6,
                UserName = "Duplicate.User",
                RecordId = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 6,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 7,
                UserName = "rod.smith",
                RecordId = new Guid("671d24b5-32d7-4ed5-8d37-e9380f1209b1"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 7,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 8,
                UserName = "randy.gradishar",
                RecordId = new Guid("9a22ce11-322f-477d-b62e-296d2f8794e0"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 8,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 9,
                UserName = "floyd.little",
                RecordId = new Guid("c379c89c-3000-49a6-be25-10ce21a1db62"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 9,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 10,
                UserName = "steve.atwater",
                RecordId = new Guid("4a861fc4-409e-48f2-a211-5d6cd9833ed0"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 10,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 11,
                UserName = "ed.mccaffrey",
                RecordId = new Guid("ee0e2b2a-48d2-4d5f-863e-ab3bb5ad3547"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 11,ModifiedDate = _standardModifiedDate
            },
            new()
            {
                BusinessEntityId = 12,
                UserName = "bill.romanowski",
                RecordId = new Guid("207a0b49-4e9b-4868-9831-7083399f1fd5"),
                PasswordHash = "$2a$11$TsEBk0KOhuIXQZe0KHcSdu05/5oj3iWPRS9TZ8M2TTDFAjRwmk8eK",
                PrimaryEmailAddressId = 12,ModifiedDate = _standardModifiedDate
            },
        });

        DbContext.SecurityFunctions.AddRange(new List<SecurityFunctionEntity>
        {
            new() {Id = 10001, RecordId = new Guid("e5be9896-0a6a-4c85-a87c-73088de1c595"), Name = "Super Secret Rights"},
            new() {Id = 10002, RecordId = new Guid("8ff2db56-dcc6-4780-bfaf-dfdd689b77ee"), Name = "Medium Secret Rights"}
        });

        DbContext.SecurityGroups.AddRange(new List<SecurityGroupEntity>
        {
            new(){Id = 101, RecordId = new Guid("4c33e231-d072-4761-b534-e3bd72259002"), Name = "Global Administrators"},
            new(){Id = 102, RecordId = new Guid("ad38c9d3-4080-408a-b29c-b9f835833dff"), Name = "Help Desk Administrators"},
            new(){Id = 103, RecordId = new Guid("d8fe6946-91f4-46fd-86c2-a2816b5f50ca"), Name = "AdventureWorks - All Employees"},
            new(){Id = 104, RecordId = new Guid("adf7f7f7-6cd5-4936-be56-4d16df156380"), Name = "AdventureWorks - All Customers"}
        });

        DbContext.SecurityRoles.AddRange(new List<SecurityRoleEntity>
        {
            new() {Id = 10, RecordId = new Guid("9f0068a4-46a1-44bf-a9a0-b6a1e25a2108"),Name = "Global Administrator"},
            new() {Id = 11, RecordId = new Guid("54521272-7763-467d-8e04-1573ea0dae56"),Name = "Help Desk Administrator" },
            new() {Id = 12, RecordId = new Guid("4a3c7403-e2fa-4017-8600-f4deb90feb16"),Name = "An AdventureWorks Employee"},
            new() {Id = 13, RecordId = new Guid("de6dd70f-9b2d-4fd7-b7b9-0aed5c36b669"),Name = "An AdventureWorks Customer"}
        });

        DbContext.SecurityGroupSecurityFunctions.AddRange(new List<SecurityGroupSecurityFunctionEntity>
        {
            new(){Id = 1, RecordId = new Guid("10f64664-dc02-4544-bf93-29319073665c"), FunctionId = 10001, GroupId = 101},
            new(){Id = 2, RecordId = new Guid("3f5032ad-7979-442f-8fce-a00fd2da099c"), FunctionId = 10002, GroupId = 102}
        });

        DbContext.SecurityGroupSecurityRoles.AddRange(new List<SecurityGroupSecurityRoleEntity>
        {
            new(){Id = 1, RecordId = new Guid("72933242-c001-433d-b3e0-fc1de8c7fba7"), RoleId = 10, GroupId = 101},
            new(){Id = 2, RecordId = new Guid("fb6acdde-1f3a-4fa3-954b-b135c4f78f2a"), RoleId = 11, GroupId = 101},
            new(){Id = 3, RecordId = new Guid("d5a37ede-afc5-4e0d-bd54-573efee1bbb7"), RoleId = 12, GroupId = 101},
            new(){Id = 4, RecordId = new Guid("86ab5de4-01c5-4acf-9105-68626d307101"), RoleId = 11, GroupId = 102},
            new(){Id = 5, RecordId = new Guid("afe02506-94c9-430f-b1ce-f0f5a31f943b"), RoleId = 12, GroupId = 102},
            new(){Id = 6, RecordId = new Guid("0209cdfc-4aa2-48e9-b8e1-e97932003c54"), RoleId = 12, GroupId = 103},
            new(){Id = 7, RecordId = new Guid("978a4541-7520-4d75-8498-4aec855f3ef2"), RoleId = 13, GroupId = 104},
        });

        DbContext.SecurityGroupUserAccounts.AddRange(new List<SecurityGroupUserAccountEntity>
        {
            new (){Id = 100001, RecordId = new Guid("9064674c-ae79-41ac-b0eb-ebe2fd6bcddf"), GroupId = 101, BusinessEntityId = 1},
            new (){Id = 100002, RecordId = new Guid("30e6ebf9-0867-49ec-be12-8d7e8bc517a1"), GroupId = 102, BusinessEntityId = 2},
            new (){Id = 100003, RecordId = new Guid("3f30f344-6119-4945-b4a3-c114d296ab66"), GroupId = 102, BusinessEntityId = 3},
            new (){Id = 100004, RecordId = new Guid("e4064490-04f0-4e60-8a86-9894df636a97"), GroupId = 103, BusinessEntityId = 4},
            new (){Id = 100005, RecordId = new Guid("339cdb3e-1c4e-4ba3-8e51-5d9f1434ad2c"), GroupId = 104, BusinessEntityId = 7}
        });

        DbContext.SaveChanges();
    }

    protected void LoadMockPeople()
    {
        DbContext.BusinessEntities.AddRange(new List<BusinessEntity>
        {
            new(){BusinessEntityId = 1, Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 2, Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 3, Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 4, Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 5, Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 6, Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 7, Rowguid = new Guid("671d24b5-32d7-4ed5-8d37-e9380f1209b1"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 8, Rowguid = new Guid("9a22ce11-322f-477d-b62e-296d2f8794e0"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 9, Rowguid = new Guid("c379c89c-3000-49a6-be25-10ce21a1db62"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 10, Rowguid = new Guid("4a861fc4-409e-48f2-a211-5d6cd9833ed0"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 11, Rowguid = new Guid("ee0e2b2a-48d2-4d5f-863e-ab3bb5ad3547"),ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 12, Rowguid = new Guid("207a0b49-4e9b-4868-9831-7083399f1fd5"),ModifiedDate = _standardModifiedDate}
        });

        DbContext.Persons.AddRange(new List<Person>
        {
            new (){BusinessEntityId = 1, FirstName = "John", LastName = "Elway", Rowguid = new Guid("7a80b0a7-1122-49e3-875b-95cc9fcae017"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 2, FirstName = "Terrell", LastName = "Davis", Rowguid = new Guid("87c347f0-e6d1-46bc-b510-ff2f9be50d82"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 3, FirstName = "Shannon", LastName = "Sharpe", Rowguid = new Guid("94159810-21c3-4666-ba28-04911f05215e"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 4, FirstName = "Emmitt", LastName = "Smith", Rowguid = new Guid("1bfe2f92-cf14-4258-a634-14ed56dbad69"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 5, FirstName = "Duplicate", LastName = "User", Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 6, FirstName = "Duplicate", LastName = "User", Rowguid = new Guid("86272e78-b76f-40ea-a706-a3d03d2c691c"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 7, FirstName = "Rod", LastName = "Smith", Rowguid = new Guid("671d24b5-32d7-4ed5-8d37-e9380f1209b1"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 8, FirstName = "Randy", LastName = "Gradishar", Rowguid = new Guid("9a22ce11-322f-477d-b62e-296d2f8794e0"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 9, FirstName = "Floyd", LastName = "Little", Rowguid = new Guid("c379c89c-3000-49a6-be25-10ce21a1db62"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 10, FirstName = "Steve", LastName = "Atwater", Rowguid = new Guid("4a861fc4-409e-48f2-a211-5d6cd9833ed0"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 11, FirstName = "Ed", LastName = "McCaffrey", Rowguid = new Guid("ee0e2b2a-48d2-4d5f-863e-ab3bb5ad3547"), ModifiedDate = _standardModifiedDate},
            new (){BusinessEntityId = 12, FirstName = "Bill", LastName = "Romanowski", Rowguid = new Guid("207a0b49-4e9b-4868-9831-7083399f1fd5"), ModifiedDate = _standardModifiedDate}
        });
        
        DbContext.EmailAddresses.AddRange(new List<EmailAddress>
        {
            new(){BusinessEntityId = 1, EmailAddressId = 1, EmailAddressName = "john.elway@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 2, EmailAddressId = 2, EmailAddressName = "terrell.davis@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 3, EmailAddressId = 3, EmailAddressName = "shannon.sharpe@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 4, EmailAddressId = 4, EmailAddressName = "emmitt.smith@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 5, EmailAddressId = 5, EmailAddressName = "Duplicate.User@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 6, EmailAddressId = 6, EmailAddressName = "Duplicate.User@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 7, EmailAddressId = 7, EmailAddressName = "rod.smith@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 8, EmailAddressId = 8, EmailAddressName = "randy.gradishar@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 9, EmailAddressId = 9, EmailAddressName = "floyd.little@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 10, EmailAddressId = 10, EmailAddressName = "steve.atwater@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 11, EmailAddressId = 11, EmailAddressName = "ed.mccaffrey@adventure-works.com",ModifiedDate = _standardModifiedDate},
            new(){BusinessEntityId = 12, EmailAddressId = 12, EmailAddressName = "bill.romanowski@adventure-works.com",ModifiedDate = _standardModifiedDate},
        });

        DbContext.SaveChanges();
    }
}
