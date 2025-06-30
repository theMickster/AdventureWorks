using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

[ExcludeFromCodeCoverage]
public sealed class EmployeeRepositoryTests : PersistenceUnitTestBase
{
    private readonly EmployeeRepository _sut;

    public EmployeeRepositoryTests()
    {
        _sut = new EmployeeRepository(DbContext);

        // Seed prerequisite data
        SeedPrerequisiteData();
    }

    private void SeedPrerequisiteData()
    {
        // Seed AddressTypes
        DbContext.AddressTypes.AddRange(new List<AddressTypeEntity>
        {
            new() { AddressTypeId = 1, Name = "Home", ModifiedDate = StandardModifiedDate },
            new() { AddressTypeId = 2, Name = "Work", ModifiedDate = StandardModifiedDate }
        });

        // Seed PhoneNumberTypes
        DbContext.PhoneNumberTypes.AddRange(new List<PhoneNumberTypeEntity>
        {
            new() { PhoneNumberTypeId = 1, Name = "Cell", ModifiedDate = StandardModifiedDate },
            new() { PhoneNumberTypeId = 2, Name = "Home", ModifiedDate = StandardModifiedDate }
        });

        // Seed StateProvinces for addresses
        DbContext.CountryRegions.Add(new CountryRegionEntity
        {
            CountryRegionCode = "US",
            Name = "United States",
            ModifiedDate = StandardModifiedDate
        });

        DbContext.StateProvinces.Add(new StateProvinceEntity
        {
            StateProvinceId = 79,
            StateProvinceCode = "WA",
            CountryRegionCode = "US",
            Name = "Washington",
            TerritoryId = 1,
            ModifiedDate = StandardModifiedDate
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public void Type_has_correct_structure()
    {
        using (new AssertionScope())
        {
            typeof(EmployeeRepository)
                .Should().Implement<IEmployeeRepository>();

            typeof(EmployeeRepository)
                .IsDefined(typeof(ServiceLifetimeScopedAttribute), false)
                .Should().BeTrue();
        }
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_null_employeeEntity_throws_exceptionAsync()
    {
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();

        Func<Task> act = async () => await _sut.CreateEmployeeWithPersonAsync(
            null!,
            personEntity,
            personPhone,
            emailAddress,
            address,
            1,
            StandardModifiedDate,
            Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("employeeEntity");
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_null_personEntity_throws_exceptionAsync()
    {
        var employeeEntity = CreateTestEmployeeEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();

        Func<Task> act = async () => await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            null!,
            personPhone,
            emailAddress,
            address,
            1,
            StandardModifiedDate,
            Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("personEntity");
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_null_personPhone_throws_exceptionAsync()
    {
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();

        Func<Task> act = async () => await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            null!,
            emailAddress,
            address,
            1,
            StandardModifiedDate,
            Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("personPhone");
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_null_emailAddress_throws_exceptionAsync()
    {
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var address = CreateTestAddress();

        Func<Task> act = async () => await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            null!,
            address,
            1,
            StandardModifiedDate,
            Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("emailAddress");
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_null_address_throws_exceptionAsync()
    {
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();

        Func<Task> act = async () => await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            emailAddress,
            null!,
            1,
            StandardModifiedDate,
            Guid.NewGuid());

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("address");
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_creates_complete_entity_graphAsync()
    {
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();
        var rowGuid = Guid.NewGuid();

        var businessEntityId = await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            emailAddress,
            address,
            addressTypeId: 2,
            StandardModifiedDate,
            rowGuid);

        using (new AssertionScope())
        {
            // Verify BusinessEntity was created
            businessEntityId.Should().BeGreaterThan(0);

            var createdBusinessEntity = await DbContext.BusinessEntities
                .FirstOrDefaultAsync(be => be.BusinessEntityId == businessEntityId);
            createdBusinessEntity.Should().NotBeNull();
            createdBusinessEntity!.Rowguid.Should().Be(rowGuid);

            // Verify Person was created
            var createdPerson = await DbContext.Persons
                .FirstOrDefaultAsync(p => p.BusinessEntityId == businessEntityId);
            createdPerson.Should().NotBeNull();
            createdPerson!.FirstName.Should().Be("John");
            createdPerson.LastName.Should().Be("Doe");
            createdPerson.PersonTypeId.Should().Be(2); // Employee person type

            // Verify Employee was created
            var createdEmployee = await DbContext.Employees
                .FirstOrDefaultAsync(e => e.BusinessEntityId == businessEntityId);
            createdEmployee.Should().NotBeNull();
            createdEmployee!.NationalIdnumber.Should().Be("123456789");
            createdEmployee.LoginId.Should().Be("adventure-works\\john.doe");
            createdEmployee.JobTitle.Should().Be("Software Engineer");
            createdEmployee.CurrentFlag.Should().BeTrue();
            createdEmployee.VacationHours.Should().Be(0);
            createdEmployee.SickLeaveHours.Should().Be(0);

            // Verify PersonPhone was created
            var createdPhone = await DbContext.PersonPhones
                .FirstOrDefaultAsync(pp => pp.BusinessEntityId == businessEntityId);
            createdPhone.Should().NotBeNull();
            createdPhone!.PhoneNumber.Should().Be("555-123-4567");
            createdPhone.PhoneNumberTypeId.Should().Be(1);

            // Verify EmailAddress was created
            var createdEmail = await DbContext.EmailAddresses
                .FirstOrDefaultAsync(ea => ea.BusinessEntityId == businessEntityId);
            createdEmail.Should().NotBeNull();
            createdEmail!.EmailAddressName.Should().Be("john.doe@adventure-works.com");

            // Verify Address was created
            var createdAddress = await DbContext.Addresses
                .FirstOrDefaultAsync(a => a.AddressLine1 == "123 Main Street");
            createdAddress.Should().NotBeNull();
            createdAddress!.City.Should().Be("Seattle");
            createdAddress.PostalCode.Should().Be("98101");
            createdAddress.StateProvinceId.Should().Be(79);

            // Verify BusinessEntityAddress was created
            var createdBusinessEntityAddress = await DbContext.BusinessEntityAddresses
                .FirstOrDefaultAsync(bea => bea.BusinessEntityId == businessEntityId);
            createdBusinessEntityAddress.Should().NotBeNull();
            createdBusinessEntityAddress!.AddressId.Should().Be(createdAddress.AddressId);
            createdBusinessEntityAddress.AddressTypeId.Should().Be(2);
        }
    }

    [Fact]
    public async Task CreateEmployeeWithPersonAsync_sets_system_fieldsAsync()
    {
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();
        var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var rowGuid = Guid.NewGuid();

        var businessEntityId = await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            emailAddress,
            address,
            addressTypeId: 1,
            testDate,
            rowGuid);

        using (new AssertionScope())
        {
            var createdPerson = await DbContext.Persons
                .FirstAsync(p => p.BusinessEntityId == businessEntityId);
            createdPerson.ModifiedDate.Should().Be(testDate);
            createdPerson.NameStyle.Should().BeFalse();
            createdPerson.EmailPromotion.Should().Be(0);
            createdPerson.Rowguid.Should().NotBeEmpty();

            var createdEmployee = await DbContext.Employees
                .FirstAsync(e => e.BusinessEntityId == businessEntityId);
            createdEmployee.ModifiedDate.Should().Be(testDate);
            createdEmployee.Rowguid.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_returns_employee_with_person_dataAsync()
    {
        // Create an employee first
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();

        var businessEntityId = await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            emailAddress,
            address,
            addressTypeId: 1,
            StandardModifiedDate,
            Guid.NewGuid());

        // Now retrieve it
        var result = await _sut.GetEmployeeByIdAsync(businessEntityId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(businessEntityId);
            result.NationalIdnumber.Should().Be("123456789");
            result.JobTitle.Should().Be("Software Engineer");

            // Verify Person data is included
            result.PersonBusinessEntity.Should().NotBeNull();
            result.PersonBusinessEntity!.FirstName.Should().Be("John");
            result.PersonBusinessEntity.LastName.Should().Be("Doe");
        }
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_returns_null_for_nonexistent_idAsync()
    {
        var result = await _sut.GetEmployeeByIdAsync(99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_uses_no_trackingAsync()
    {
        // Create an employee first
        var employeeEntity = CreateTestEmployeeEntity();
        var personEntity = CreateTestPersonEntity();
        var personPhone = CreateTestPersonPhone();
        var emailAddress = CreateTestEmailAddress();
        var address = CreateTestAddress();

        var businessEntityId = await _sut.CreateEmployeeWithPersonAsync(
            employeeEntity,
            personEntity,
            personPhone,
            emailAddress,
            address,
            addressTypeId: 1,
            StandardModifiedDate,
            Guid.NewGuid());

        // Retrieve the employee
        var result = await _sut.GetEmployeeByIdAsync(businessEntityId);

        // Verify it's not tracked
        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }

    // Helper methods to create test entities
    private static EmployeeEntity CreateTestEmployeeEntity()
    {
        return new EmployeeEntity
        {
            NationalIdnumber = "123456789",
            LoginId = "adventure-works\\john.doe",
            JobTitle = "Software Engineer",
            BirthDate = new DateTime(1990, 5, 15),
            HireDate = new DateTime(2020, 1, 10),
            MaritalStatus = "M",
            Gender = "M",
            OrganizationLevel = 2
        };
    }

    private static PersonEntity CreateTestPersonEntity()
    {
        return new PersonEntity
        {
            FirstName = "John",
            LastName = "Doe",
            MiddleName = "Michael",
            Title = "Mr.",
            Suffix = "Jr."
        };
    }

    private static PersonPhone CreateTestPersonPhone()
    {
        return new PersonPhone
        {
            PhoneNumber = "555-123-4567",
            PhoneNumberTypeId = 1
        };
    }

    private static EmailAddressEntity CreateTestEmailAddress()
    {
        return new EmailAddressEntity
        {
            EmailAddressName = "john.doe@adventure-works.com"
        };
    }

    private static AddressEntity CreateTestAddress()
    {
        return new AddressEntity
        {
            AddressLine1 = "123 Main Street",
            AddressLine2 = "Apt 4B",
            City = "Seattle",
            PostalCode = "98101",
            StateProvinceId = 79
        };
    }
}
