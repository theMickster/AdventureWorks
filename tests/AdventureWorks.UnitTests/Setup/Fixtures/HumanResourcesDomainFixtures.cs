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
}
