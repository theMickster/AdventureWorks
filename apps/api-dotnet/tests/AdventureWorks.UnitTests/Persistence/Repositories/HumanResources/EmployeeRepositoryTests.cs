using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Common.Constants;
using AdventureWorks.Common.Filtering;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories;
using AdventureWorks.UnitTests.Setup.Fixtures;
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
    public async Task GetEmployeeByIdAsync_uses_trackingAsync()
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

        // Verify it's tracked
        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Unchanged);
    }

    #region GetEmployeesAsync Tests

    [Fact]
    public async Task GetEmployeesAsync_returns_paginated_employees_with_total_countAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 3 };

        var (result, totalCount) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            totalCount.Should().Be(5);
            result.Should().AllSatisfy(e => e.PersonBusinessEntity.Should().NotBeNull());
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_honors_page_size_and_numberAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 2, PageSize = 2 };

        var (result, totalCount) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            totalCount.Should().Be(5);
            result[0].BusinessEntityId.Should().Be(3);
            result[1].BusinessEntityId.Should().Be(4);
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_sorts_by_BusinessEntityId_ascendingAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "id",
            SortOrder = SortedResultConstants.Ascending
        };

        var (result, _) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInAscendingOrder(e => e.BusinessEntityId);
            result[0].BusinessEntityId.Should().Be(1);
            result[^1].BusinessEntityId.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_sorts_by_BusinessEntityId_descendingAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "id",
            SortOrder = SortedResultConstants.Descending
        };

        var (result, _) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInDescendingOrder(e => e.BusinessEntityId);
            result[0].BusinessEntityId.Should().Be(5);
            result[^1].BusinessEntityId.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_sorts_by_FirstName_ascendingAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "firstName",
            SortOrder = SortedResultConstants.Ascending
        };

        var (result, _) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInAscendingOrder(e => e.PersonBusinessEntity.FirstName);
            result[0].PersonBusinessEntity.FirstName.Should().Be("Alice");
            result[^1].PersonBusinessEntity.FirstName.Should().Be("Edward");
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_sorts_by_LastName_descendingAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "lastName",
            SortOrder = SortedResultConstants.Descending
        };

        var (result, _) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeInDescendingOrder(e => e.PersonBusinessEntity.LastName);
            result[0].PersonBusinessEntity.LastName.Should().Be("Evans");
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_returns_empty_list_when_no_employeesAsync()
    {
        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };

        var (result, totalCount) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_includes_person_and_email_dataAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity(1, "Test", "User", "Tester", "999999999", "adventure-works\\test.user", "test.user@adventure-works.com");
        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };

        var (result, _) = await _sut.GetEmployeesAsync(parameters);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].PersonBusinessEntity.Should().NotBeNull();
            result[0].PersonBusinessEntity.FirstName.Should().Be("Test");
            result[0].PersonBusinessEntity.LastName.Should().Be("User");
            result[0].PersonBusinessEntity.EmailAddresses.Should().NotBeEmpty();
            result[0].PersonBusinessEntity.EmailAddresses.First().EmailAddressName.Should().Be("test.user@adventure-works.com");
        }
    }

    [Fact]
    public async Task GetEmployeesAsync_uses_no_trackingAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };

        var (result, _) = await _sut.GetEmployeesAsync(parameters);

        var entry = DbContext.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region SearchEmployeesAsync Tests

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_IdAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { Id = 3 };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].BusinessEntityId.Should().Be(3);
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_FirstNameAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { FirstName = "bob" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].PersonBusinessEntity.FirstName.Should().Be("Bob");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_LastNameAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { LastName = "davis" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].PersonBusinessEntity.LastName.Should().Be("Davis");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_JobTitleAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { JobTitle = "developer" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].JobTitle.Should().Be("Developer");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_EmailAddressAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { EmailAddress = "charlie.chen" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].PersonBusinessEntity.EmailAddresses.First().EmailAddressName.Should().Contain("charlie.chen");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_NationalIdNumberAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { NationalIdNumber = "444444444" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].NationalIdnumber.Should().Be("444444444");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_filters_by_LoginIdAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { LoginId = "alice.anderson" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].LoginId.Should().Contain("alice.anderson");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_combines_multiple_filtersAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel
        {
            FirstName = "bob",
            LastName = "brown",
            JobTitle = "developer"
        };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].PersonBusinessEntity.FirstName.Should().Be("Bob");
            result[0].PersonBusinessEntity.LastName.Should().Be("Brown");
            result[0].JobTitle.Should().Be("Developer");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_returns_empty_when_no_matchesAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { FirstName = "NonExistent" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().BeEmpty();
            totalCount.Should().Be(0);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_case_insensitive_searchAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel { FirstName = "CHARLIE" };

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].PersonBusinessEntity.FirstName.Should().Be("Charlie");
            totalCount.Should().Be(1);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_honors_paging_parametersAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 2, PageSize = 2 };
        var searchModel = new EmployeeSearchModel();

        var (result, totalCount) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        using (new AssertionScope())
        {
            result.Count.Should().Be(2);
            totalCount.Should().Be(5);
        }
    }

    [Fact]
    public async Task SearchEmployeesAsync_sorts_by_FirstName_ascendingAsync()
    {
        var employees = HumanResourcesDomainFixtures.GetEmployeeListForPaging();
        DbContext.Employees.AddRange(employees);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter
        {
            PageNumber = 1,
            PageSize = 10,
            OrderBy = "firstName",
            SortOrder = SortedResultConstants.Ascending
        };
        var searchModel = new EmployeeSearchModel();

        var (result, _) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        result.Should().BeInAscendingOrder(e => e.PersonBusinessEntity.FirstName);
    }

    [Fact]
    public async Task SearchEmployeesAsync_uses_no_trackingAsync()
    {
        var employee = HumanResourcesDomainFixtures.GetCompleteEmployeeEntity();
        DbContext.Employees.Add(employee);
        await DbContext.SaveChangesAsync();

        var parameters = new EmployeeParameter { PageNumber = 1, PageSize = 10 };
        var searchModel = new EmployeeSearchModel();

        var (result, _) = await _sut.SearchEmployeesAsync(parameters, searchModel);

        var entry = DbContext.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region GetEmployeeAddressesAsync Tests

    [Fact]
    public async Task GetEmployeeAddressesAsync_returns_all_addresses_for_employeeAsync()
    {
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

        var secondAddress = new AddressEntity
        {
            AddressLine1 = "456 Oak Avenue",
            City = "Portland",
            PostalCode = "97201",
            StateProvinceId = 79,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Addresses.Add(secondAddress);
        await DbContext.SaveChangesAsync();

        var secondBea = new BusinessEntityAddressEntity
        {
            BusinessEntityId = businessEntityId,
            AddressId = secondAddress.AddressId,
            AddressTypeId = 2,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.BusinessEntityAddresses.Add(secondBea);
        await DbContext.SaveChangesAsync();

        var result = await _sut.GetEmployeeAddressesAsync(businessEntityId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().AllSatisfy(bea =>
            {
                bea.Address.Should().NotBeNull();
                bea.AddressType.Should().NotBeNull();
                bea.Address.StateProvince.Should().NotBeNull();
                bea.Address.StateProvince.CountryRegion.Should().NotBeNull();
            });
        }
    }

    [Fact]
    public async Task GetEmployeeAddressesAsync_returns_empty_when_no_addressesAsync()
    {
        var result = await _sut.GetEmployeeAddressesAsync(99999);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEmployeeAddressesAsync_includes_full_address_hierarchyAsync()
    {
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

        var result = await _sut.GetEmployeeAddressesAsync(businessEntityId);

        using (new AssertionScope())
        {
            result.Should().HaveCount(1);
            result[0].Address.AddressLine1.Should().Be("123 Main Street");
            result[0].Address.City.Should().Be("Seattle");
            result[0].Address.StateProvince.Name.Should().Be("Washington");
            result[0].Address.StateProvince.CountryRegion.Name.Should().Be("United States");
            result[0].AddressType.Name.Should().Be("Home");
        }
    }

    [Fact]
    public async Task GetEmployeeAddressesAsync_uses_no_trackingAsync()
    {
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

        var result = await _sut.GetEmployeeAddressesAsync(businessEntityId);

        var entry = DbContext.Entry(result[0]);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region GetEmployeeAddressByIdAsync Tests

    [Fact]
    public async Task GetEmployeeAddressByIdAsync_returns_specific_addressAsync()
    {
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

        var createdAddress = await DbContext.Addresses
            .FirstAsync(a => a.AddressLine1 == "123 Main Street");

        var result = await _sut.GetEmployeeAddressByIdAsync(businessEntityId, createdAddress.AddressId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(businessEntityId);
            result.AddressId.Should().Be(createdAddress.AddressId);
            result.Address.Should().NotBeNull();
            result.AddressType.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetEmployeeAddressByIdAsync_returns_null_when_not_foundAsync()
    {
        var result = await _sut.GetEmployeeAddressByIdAsync(99999, 99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeAddressByIdAsync_returns_null_when_address_not_for_employeeAsync()
    {
        var employee1 = CreateTestEmployeeEntity();
        var person1 = CreateTestPersonEntity();
        var phone1 = CreateTestPersonPhone();
        var email1 = CreateTestEmailAddress();
        var address1 = CreateTestAddress();

        var businessEntityId1 = await _sut.CreateEmployeeWithPersonAsync(
            employee1, person1, phone1, email1, address1, 1, StandardModifiedDate, Guid.NewGuid());

        var employee2 = new EmployeeEntity
        {
            NationalIdnumber = "987654321",
            LoginId = "adventure-works\\jane.doe",
            JobTitle = "Manager",
            BirthDate = new DateTime(1985, 3, 20),
            HireDate = new DateTime(2019, 5, 1),
            MaritalStatus = "S",
            Gender = "F",
            OrganizationLevel = 3
        };
        var person2 = new PersonEntity { FirstName = "Jane", LastName = "Doe" };
        var phone2 = new PersonPhone { PhoneNumber = "555-987-6543", PhoneNumberTypeId = 1 };
        var email2 = new EmailAddressEntity { EmailAddressName = "jane.doe@adventure-works.com" };
        var address2 = new AddressEntity
        {
            AddressLine1 = "789 Elm Street",
            City = "Portland",
            PostalCode = "97201",
            StateProvinceId = 79
        };

        var businessEntityId2 = await _sut.CreateEmployeeWithPersonAsync(
            employee2, person2, phone2, email2, address2, 1, StandardModifiedDate, Guid.NewGuid());

        var employee2Address = await DbContext.Addresses.FirstAsync(a => a.AddressLine1 == "789 Elm Street");

        var result = await _sut.GetEmployeeAddressByIdAsync(businessEntityId1, employee2Address.AddressId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeAddressByIdAsync_includes_full_address_hierarchyAsync()
    {
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

        var createdAddress = await DbContext.Addresses.FirstAsync(a => a.AddressLine1 == "123 Main Street");

        var result = await _sut.GetEmployeeAddressByIdAsync(businessEntityId, createdAddress.AddressId);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.Address.AddressLine1.Should().Be("123 Main Street");
            result.Address.StateProvince.Should().NotBeNull();
            result.Address.StateProvince.Name.Should().Be("Washington");
            result.Address.StateProvince.CountryRegion.Should().NotBeNull();
            result.Address.StateProvince.CountryRegion.Name.Should().Be("United States");
            result.AddressType.Should().NotBeNull();
            result.AddressType.Name.Should().Be("Home");
        }
    }

    [Fact]
    public async Task GetEmployeeAddressByIdAsync_uses_no_trackingAsync()
    {
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

        var createdAddress = await DbContext.Addresses.FirstAsync(a => a.AddressLine1 == "123 Main Street");

        var result = await _sut.GetEmployeeAddressByIdAsync(businessEntityId, createdAddress.AddressId);

        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

    #region GetEmployeeByIdWithDepartmentHistoryAsync Tests

    [Fact]
    public async Task GetEmployeeByIdWithDepartmentHistoryAsync_returns_employee_with_department_historyAsync()
    {
        // Seed departments and shifts
        var department = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Departments.Add(department);

        var shift = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Shifts.Add(shift);
        await DbContext.SaveChangesAsync();

        // Create employee
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

        // Add department history
        // Retrieve the employee to add history record
        var employee = await DbContext.Employees.FindAsync(businessEntityId);
        employee!.EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
        {
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = new DateTime(2020, 1, 10),
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            }
        };
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetEmployeeByIdWithDepartmentHistoryAsync(businessEntityId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(businessEntityId);
            result.EmployeeDepartmentHistory.Should().NotBeNull();
            result.EmployeeDepartmentHistory.Should().HaveCount(1);

            var history = result.EmployeeDepartmentHistory.First();
            history.Department.Should().NotBeNull();
            history.Department.Name.Should().Be("Engineering");
            history.Shift.Should().NotBeNull();
            history.Shift.Name.Should().Be("Day");
            history.StartDate.Should().Be(new DateTime(2020, 1, 10));
            history.EndDate.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetEmployeeByIdWithDepartmentHistoryAsync_returns_null_for_nonexistent_employeeAsync()
    {
        var result = await _sut.GetEmployeeByIdWithDepartmentHistoryAsync(99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeByIdWithDepartmentHistoryAsync_includes_multiple_history_recordsAsync()
    {
        // Seed departments and shifts
        var dept1 = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = StandardModifiedDate
        };
        var dept2 = new DepartmentEntity
        {
            DepartmentId = 2,
            Name = "Marketing",
            GroupName = "Sales and Marketing",
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Departments.AddRange(dept1, dept2);

        var shift = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Shifts.Add(shift);
        await DbContext.SaveChangesAsync();

        // Create employee
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

        // Add two department history records (transfer scenario)
        // Retrieve the employee to add history records
        var employee = await DbContext.Employees.FindAsync(businessEntityId);
        employee!.EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
        {
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = new DateTime(2020, 1, 10),
                EndDate = new DateTime(2022, 6, 30),
                ModifiedDate = StandardModifiedDate
            },
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 2,
                ShiftId = 1,
                StartDate = new DateTime(2022, 7, 1),
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            }
        };
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetEmployeeByIdWithDepartmentHistoryAsync(businessEntityId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.EmployeeDepartmentHistory.Should().HaveCount(2);
            result.EmployeeDepartmentHistory.Should().Contain(h => h.Department.Name == "Engineering" && h.EndDate.HasValue);
            result.EmployeeDepartmentHistory.Should().Contain(h => h.Department.Name == "Marketing" && h.EndDate == null);
        }
    }

    [Fact]
    public async Task GetEmployeeByIdWithDepartmentHistoryAsync_uses_trackingAsync()
    {
        // Seed department and shift
        var department = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Departments.Add(department);

        var shift = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Shifts.Add(shift);
        await DbContext.SaveChangesAsync();

        // Create employee with department history
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

        // Retrieve the employee to add history record
        var employee = await DbContext.Employees.FindAsync(businessEntityId);
        employee!.EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
        {
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = new DateTime(2020, 1, 10),
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            }
        };
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetEmployeeByIdWithDepartmentHistoryAsync(businessEntityId);

        // Assert - Should be tracked for update scenarios
        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Unchanged);
    }

    #endregion

    #region GetEmployeeByIdWithLifecycleDataAsync Tests

    [Fact]
    public async Task GetEmployeeByIdWithLifecycleDataAsync_returns_employee_with_full_lifecycle_dataAsync()
    {
        // Seed departments, shifts
        var department = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Departments.Add(department);

        var shift = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Shifts.Add(shift);
        await DbContext.SaveChangesAsync();

        // Create employee
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

        // Add department history and pay history through employee entity
        var employee = await DbContext.Employees.FindAsync(businessEntityId);
        employee!.EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
        {
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = new DateTime(2020, 1, 10),
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            }
        };
        employee.EmployeePayHistory = new List<EmployeePayHistory>
        {
            new EmployeePayHistory
            {
                BusinessEntityId = businessEntityId,
                RateChangeDate = new DateTime(2020, 1, 10),
                Rate = 45.00m,
                PayFrequency = 2,
                ModifiedDate = StandardModifiedDate
            }
        };
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetEmployeeByIdWithLifecycleDataAsync(businessEntityId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.BusinessEntityId.Should().Be(businessEntityId);

            // Verify Person data
            result.PersonBusinessEntity.Should().NotBeNull();
            result.PersonBusinessEntity.FirstName.Should().Be("John");
            result.PersonBusinessEntity.LastName.Should().Be("Doe");

            // Verify Department History
            result.EmployeeDepartmentHistory.Should().NotBeNull();
            result.EmployeeDepartmentHistory.Should().HaveCount(1);
            result.EmployeeDepartmentHistory.First().Department.Should().NotBeNull();
            result.EmployeeDepartmentHistory.First().Department.Name.Should().Be("Engineering");
            result.EmployeeDepartmentHistory.First().Shift.Should().NotBeNull();
            result.EmployeeDepartmentHistory.First().Shift.Name.Should().Be("Day");

            // Verify Pay History
            result.EmployeePayHistory.Should().NotBeNull();
            result.EmployeePayHistory.Should().HaveCount(1);
            result.EmployeePayHistory.First().Rate.Should().Be(45.00m);
            result.EmployeePayHistory.First().PayFrequency.Should().Be(2);
        }
    }

    [Fact]
    public async Task GetEmployeeByIdWithLifecycleDataAsync_returns_null_for_nonexistent_employeeAsync()
    {
        var result = await _sut.GetEmployeeByIdWithLifecycleDataAsync(99999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeByIdWithLifecycleDataAsync_handles_employee_without_history_dataAsync()
    {
        // Create employee without department/pay history
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

        // Act
        var result = await _sut.GetEmployeeByIdWithLifecycleDataAsync(businessEntityId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.PersonBusinessEntity.Should().NotBeNull();
            result.EmployeeDepartmentHistory.Should().BeEmpty();
            result.EmployeePayHistory.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetEmployeeByIdWithLifecycleDataAsync_includes_multiple_pay_history_recordsAsync()
    {
        // Seed department and shift
        var department = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Departments.Add(department);

        var shift = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Shifts.Add(shift);
        await DbContext.SaveChangesAsync();

        // Create employee
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

        // Add multiple pay history records (raises scenario) through employee entity
        var employee = await DbContext.Employees.FindAsync(businessEntityId);
        employee!.EmployeePayHistory = new List<EmployeePayHistory>
        {
            new EmployeePayHistory
            {
                BusinessEntityId = businessEntityId,
                RateChangeDate = new DateTime(2020, 1, 10),
                Rate = 40.00m,
                PayFrequency = 2,
                ModifiedDate = StandardModifiedDate
            },
            new EmployeePayHistory
            {
                BusinessEntityId = businessEntityId,
                RateChangeDate = new DateTime(2021, 1, 10),
                Rate = 45.00m,
                PayFrequency = 2,
                ModifiedDate = StandardModifiedDate
            },
            new EmployeePayHistory
            {
                BusinessEntityId = businessEntityId,
                RateChangeDate = new DateTime(2022, 1, 10),
                Rate = 50.00m,
                PayFrequency = 2,
                ModifiedDate = StandardModifiedDate
            }
        };
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetEmployeeByIdWithLifecycleDataAsync(businessEntityId);

        // Assert
        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result!.EmployeePayHistory.Should().HaveCount(3);
            result.EmployeePayHistory.Should().Contain(ph => ph.Rate == 40.00m);
            result.EmployeePayHistory.Should().Contain(ph => ph.Rate == 45.00m);
            result.EmployeePayHistory.Should().Contain(ph => ph.Rate == 50.00m);
        }
    }

    [Fact]
    public async Task GetEmployeeByIdWithLifecycleDataAsync_uses_no_trackingAsync()
    {
        // Seed department and shift
        var department = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Departments.Add(department);

        var shift = new ShiftEntity
        {
            ShiftId = 1,
            Name = "Day",
            StartTime = new TimeSpan(8, 0, 0),
            EndTime = new TimeSpan(17, 0, 0),
            ModifiedDate = StandardModifiedDate
        };
        DbContext.Shifts.Add(shift);
        await DbContext.SaveChangesAsync();

        // Create employee
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

        // Retrieve the employee to add history record
        var employee = await DbContext.Employees.FindAsync(businessEntityId);
        employee!.EmployeeDepartmentHistory = new List<EmployeeDepartmentHistory>
        {
            new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = 1,
                ShiftId = 1,
                StartDate = new DateTime(2020, 1, 10),
                EndDate = null,
                ModifiedDate = StandardModifiedDate
            }
        };
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetEmployeeByIdWithLifecycleDataAsync(businessEntityId);

        // Assert - Should NOT be tracked (read-only query for status)
        var entry = DbContext.Entry(result!);
        entry.State.Should().Be(EntityState.Detached);
    }

    #endregion

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
