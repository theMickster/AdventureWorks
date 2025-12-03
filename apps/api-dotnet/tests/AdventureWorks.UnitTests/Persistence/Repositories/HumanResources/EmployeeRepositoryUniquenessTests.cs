using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Infrastructure.Persistence.Repositories;

namespace AdventureWorks.UnitTests.Persistence.Repositories.HumanResources;

/// <summary>
/// In-memory EF Core tests for the uniqueness-check methods backing CreateEmployeeValidator's
/// Rule-31/Rule-32 checks, plus direct tests of the DbUpdateException-to-ConflictException
/// message-selection logic used by <see cref="EmployeeRepository.CreateEmployeeWithPersonAsync"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class EmployeeRepositoryUniquenessTests : PersistenceUnitTestBase
{
    private readonly EmployeeRepository _sut;

    public EmployeeRepositoryUniquenessTests()
    {
        _sut = new EmployeeRepository(DbContext);

        DbContext.BusinessEntities.Add(new BusinessEntity
        {
            BusinessEntityId = 1,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.Employees.Add(new EmployeeEntity
        {
            BusinessEntityId = 1,
            NationalIdnumber = "123456789",
            LoginId = "adventure-works\\john.doe",
            JobTitle = "Software Engineer",
            BirthDate = new DateTime(1990, 1, 1),
            MaritalStatus = "S",
            Gender = "M",
            HireDate = new DateTime(1996, 7, 1),
            CurrentFlag = true,
            Rowguid = Guid.NewGuid(),
            ModifiedDate = StandardModifiedDate
        });

        DbContext.SaveChanges();
    }

    [Fact]
    public async Task NationalIdNumberExistsAsync_returns_true_when_matching_employee_existsAsync()
    {
        var result = await _sut.NationalIdNumberExistsAsync("123456789");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task NationalIdNumberExistsAsync_returns_false_when_no_matching_employee_existsAsync()
    {
        var result = await _sut.NationalIdNumberExistsAsync("999999999");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task LoginIdExistsAsync_returns_true_when_matching_employee_existsAsync()
    {
        var result = await _sut.LoginIdExistsAsync("adventure-works\\john.doe");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task LoginIdExistsAsync_returns_false_when_no_matching_employee_existsAsync()
    {
        var result = await _sut.LoginIdExistsAsync("adventure-works\\nobody");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(
        "Violation of UNIQUE KEY constraint 'AK_Employee_NationalIDNumber'. Cannot insert duplicate key.",
        "An employee with this National ID Number already exists.")]
    [InlineData(
        "Violation of UNIQUE KEY constraint 'AK_Employee_LoginID'. Cannot insert duplicate key.",
        "An employee with this Login ID already exists.")]
    [InlineData(
        "Violation of UNIQUE KEY constraint 'AK_SomeOtherTable_SomeColumn'. Cannot insert duplicate key.",
        "An employee with conflicting unique data already exists.")]
    public void BuildUniqueConstraintConflictMessage_selects_message_from_sql_exception_text(
        string sqlExceptionMessage,
        string expectedMessage)
    {
        var result = EmployeeRepository.BuildUniqueConstraintConflictMessage(sqlExceptionMessage);

        result.Should().Be(expectedMessage);
    }
}
