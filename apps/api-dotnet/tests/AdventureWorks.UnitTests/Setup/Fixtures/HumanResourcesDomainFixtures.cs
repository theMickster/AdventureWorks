using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Models.Slim;

namespace AdventureWorks.UnitTests.Setup.Fixtures;

/// <summary>
/// Test fixture providing sample data for Human Resources domain testing.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class HumanResourcesDomainFixtures : UnitTestFixtureBase
{
    internal static DateTime HumanResourcesDefaultAuditDate => new(2011, 11, 11, 11, 11, 11, DateTimeKind.Utc);

    /// <summary>
    /// Creates a valid EmployeeCreateModel with all required fields populated.
    /// </summary>
    internal static EmployeeCreateModel GetValidEmployeeCreateModel()
    {
        return new EmployeeCreateModel
        {
            // Person data
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Title = "Mr.",
            Suffix = "Jr.",

            // Employee data
            NationalIdNumber = "123456789",
            LoginId = "adventure-works\\john.doe",
            JobTitle = "Software Engineer",
            BirthDate = new DateTime(1990, 5, 15),
            HireDate = new DateTime(2020, 1, 10),
            MaritalStatus = "M",
            Gender = "M",
            OrganizationLevel = 2,

            // Contact information
            Phone = new EmployeePhoneCreateModel
            {
                PhoneNumber = "555-123-4567",
                PhoneNumberTypeId = 1
            },
            EmailAddress = "john.doe@adventure-works.com",

            // Address information
            Address = new AddressCreateModel
            {
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Apt 4B",
                City = "Seattle",
                PostalCode = "98101",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
            },
            AddressTypeId = 2
        };
    }

    /// <summary>
    /// Creates a valid EmployeeEntity with required fields populated.
    /// </summary>
    internal static EmployeeEntity GetValidEmployeeEntity(int businessEntityId = 100)
    {
        return new EmployeeEntity
        {
            BusinessEntityId = businessEntityId,
            NationalIdnumber = "123456789",
            LoginId = "adventure-works\\john.doe",
            JobTitle = "Software Engineer",
            BirthDate = new DateTime(1990, 5, 15),
            HireDate = new DateTime(2020, 1, 10),
            MaritalStatus = "M",
            Gender = "M",
            OrganizationLevel = 2,
            CurrentFlag = true,
            SalariedFlag = false,
            VacationHours = 0,
            SickLeaveHours = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a valid PersonEntity with required fields populated.
    /// </summary>
    internal static PersonEntity GetValidPersonEntity(int businessEntityId = 100)
    {
        return new PersonEntity
        {
            BusinessEntityId = businessEntityId,
            PersonTypeId = 2, // Employee
            NameStyle = false,
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Title = "Mr.",
            Suffix = "Jr.",
            EmailPromotion = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a minimal valid EmployeeCreateModel (required fields only).
    /// </summary>
    internal static EmployeeCreateModel GetMinimalValidEmployeeCreateModel()
    {
        return new EmployeeCreateModel
        {
            FirstName = "Jane",
            LastName = "Smith",
            NationalIdNumber = "987654321",
            LoginId = "adventure-works\\jane.smith",
            JobTitle = "Developer",
            BirthDate = new DateTime(1995, 3, 20),
            HireDate = new DateTime(2022, 6, 1),
            MaritalStatus = "S",
            Gender = "F",
            Phone = new EmployeePhoneCreateModel
            {
                PhoneNumber = "555-987-6543",
                PhoneNumberTypeId = 1
            },
            EmailAddress = "jane.smith@adventure-works.com",
            Address = new AddressCreateModel
            {
                AddressLine1 = "456 Oak Avenue",
                City = "Portland",
                PostalCode = "97201",
                StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
            },
            AddressTypeId = 1
        };
    }

    /// <summary>
    /// Creates an EmployeePhoneCreateModel with valid data.
    /// </summary>
    internal static EmployeePhoneCreateModel GetValidPhoneModel(
        string phoneNumber = "555-123-4567",
        int phoneNumberTypeId = 1)
    {
        return new EmployeePhoneCreateModel
        {
            PhoneNumber = phoneNumber,
            PhoneNumberTypeId = phoneNumberTypeId
        };
    }

    /// <summary>
    /// Creates an AddressCreateModel with valid data.
    /// </summary>
    internal static AddressCreateModel GetValidAddressModel(
        string city = "Seattle",
        string postalCode = "98101")
    {
        return new AddressCreateModel
        {
            AddressLine1 = "123 Main Street",
            City = city,
            PostalCode = postalCode,
            StateProvince = new GenericSlimModel { Id = 79, Name = "Washington", Code = "WA" }
        };
    }

    /// <summary>
    /// Creates a valid EmployeeUpdateModel with all required fields populated.
    /// </summary>
    internal static EmployeeUpdateModel GetValidEmployeeUpdateModel(int businessEntityId = 100)
    {
        return new EmployeeUpdateModel
        {
            Id = businessEntityId,
            FirstName = "Jane",
            LastName = "Smith",
            MiddleName = "Elizabeth",
            Title = "Dr.",
            Suffix = "III",
            MaritalStatus = "S",
            Gender = "F"
        };
    }

    /// <summary>
    /// Creates a valid EmployeeAddressUpdateModel with all required fields populated.
    /// </summary>
    internal static EmployeeAddressUpdateModel GetValidEmployeeAddressUpdateModel(int addressId = 1)
    {
        return new EmployeeAddressUpdateModel
        {
            AddressId = addressId,
            AddressLine1 = "456 Elm Street",
            AddressLine2 = "Suite 200",
            City = "Portland",
            StateProvinceId = 79,
            PostalCode = "97201"
        };
    }

    /// <summary>
    /// Creates a valid AddressEntity with related data for testing.
    /// </summary>
    internal static AddressEntity GetValidAddressEntity(int addressId = 1)
    {
        return new AddressEntity
        {
            AddressId = addressId,
            AddressLine1 = "123 Main Street",
            AddressLine2 = "Apt 4B",
            City = "Seattle",
            StateProvinceId = 79,
            PostalCode = "98101",
            Rowguid = Guid.NewGuid(),
            ModifiedDate = HumanResourcesDefaultAuditDate,
            StateProvince = new StateProvinceEntity
            {
                StateProvinceId = 79,
                StateProvinceCode = "WA",
                Name = "Washington",
                CountryRegionCode = "US",
                IsOnlyStateProvinceFlag = false,
                TerritoryId = 1,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = HumanResourcesDefaultAuditDate,
                CountryRegion = new CountryRegionEntity
                {
                    CountryRegionCode = "US",
                    Name = "United States",
                    ModifiedDate = HumanResourcesDefaultAuditDate
                }
            }
        };
    }

    /// <summary>
    /// Creates a valid BusinessEntityAddressEntity with related data for testing.
    /// </summary>
    internal static BusinessEntityAddressEntity GetValidBusinessEntityAddress(
        int businessEntityId = 100,
        int addressId = 1,
        int addressTypeId = 2)
    {
        return new BusinessEntityAddressEntity
        {
            BusinessEntityId = businessEntityId,
            AddressId = addressId,
            AddressTypeId = addressTypeId,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = HumanResourcesDefaultAuditDate,
            Address = GetValidAddressEntity(addressId),
            AddressType = new AddressTypeEntity
            {
                AddressTypeId = addressTypeId,
                Name = "Home",
                Rowguid = Guid.NewGuid(),
                ModifiedDate = HumanResourcesDefaultAuditDate
            }
        };
    }

    /// <summary>
    /// Creates a complete employee entity with person and email data for repository testing.
    /// </summary>
    internal static EmployeeEntity GetCompleteEmployeeEntity(
        int businessEntityId = 100,
        string firstName = "John",
        string lastName = "Doe",
        string jobTitle = "Software Engineer",
        string nationalIdNumber = "123456789",
        string loginId = "adventure-works\\john.doe",
        string emailAddress = "john.doe@adventure-works.com")
    {
        return new EmployeeEntity
        {
            BusinessEntityId = businessEntityId,
            NationalIdnumber = nationalIdNumber,
            LoginId = loginId,
            JobTitle = jobTitle,
            BirthDate = new DateTime(1990, 5, 15),
            HireDate = new DateTime(2020, 1, 10),
            MaritalStatus = "M",
            Gender = "M",
            OrganizationLevel = 2,
            CurrentFlag = true,
            SalariedFlag = false,
            VacationHours = 0,
            SickLeaveHours = 0,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = HumanResourcesDefaultAuditDate,
            PersonBusinessEntity = new PersonEntity
            {
                BusinessEntityId = businessEntityId,
                PersonTypeId = 2,
                NameStyle = false,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = "M",
                Title = "Mr.",
                Suffix = "Jr.",
                EmailPromotion = 0,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = HumanResourcesDefaultAuditDate,
                EmailAddresses = new List<EmailAddressEntity>
                {
                    new EmailAddressEntity
                    {
                        BusinessEntityId = businessEntityId,
                        EmailAddressId = businessEntityId,
                        EmailAddressName = emailAddress,
                        Rowguid = Guid.NewGuid(),
                        ModifiedDate = HumanResourcesDefaultAuditDate
                    }
                }
            }
        };
    }

    /// <summary>
    /// Creates a list of employee entities for pagination testing.
    /// </summary>
    internal static List<EmployeeEntity> GetEmployeeListForPaging()
    {
        return new List<EmployeeEntity>
        {
            GetCompleteEmployeeEntity(1, "Alice", "Anderson", "Manager", "111111111", "adventure-works\\alice.anderson", "alice.anderson@adventure-works.com"),
            GetCompleteEmployeeEntity(2, "Bob", "Brown", "Developer", "222222222", "adventure-works\\bob.brown", "bob.brown@adventure-works.com"),
            GetCompleteEmployeeEntity(3, "Charlie", "Chen", "Designer", "333333333", "adventure-works\\charlie.chen", "charlie.chen@adventure-works.com"),
            GetCompleteEmployeeEntity(4, "Diana", "Davis", "Analyst", "444444444", "adventure-works\\diana.davis", "diana.davis@adventure-works.com"),
            GetCompleteEmployeeEntity(5, "Edward", "Evans", "Engineer", "555555555", "adventure-works\\edward.evans", "edward.evans@adventure-works.com")
        };
    }
}
