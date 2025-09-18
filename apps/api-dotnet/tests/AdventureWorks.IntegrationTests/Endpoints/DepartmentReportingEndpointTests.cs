using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.IntegrationTests.Setup;
using AdventureWorks.Models.Features.HumanResources;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AdventureWorks.IntegrationTests.Endpoints;

public sealed class DepartmentReportingEndpointTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetDepartmentHeadcount_Anonymous_Returns401()
    {
        var client = CreateAnonymousClient();

        var response = await client.GetAsync("/api/v1.0/departments/1/headcount");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetDepartmentHeadcount_InvalidDepartmentId_Returns400(short departmentId)
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/departments/{departmentId}/headcount");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDepartmentHeadcount_MissingDepartment_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/departments/31000/headcount");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDepartmentHeadcount_WithSeededDepartment_Returns200()
    {
        var departmentId = await SeedDepartmentWithActiveEmployeeAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/departments/{departmentId}/headcount");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var model = await DeserializeAsync<DepartmentHeadcountModel>(response);

        model.Should().NotBeNull();
        model!.DepartmentId.Should().Be(departmentId);
        model.ActiveEmployeeCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDepartmentHeadcountSummary_WithSeededDepartment_Returns200()
    {
        var departmentId = await SeedDepartmentWithActiveEmployeeAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/departments/headcount-summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var models = await DeserializeAsync<List<DepartmentHeadcountSummaryModel>>(response);

        models.Should().NotBeNull();
        models!.Should().Contain(m => m.DepartmentId == departmentId && m.ActiveEmployeeCount > 0);
    }

    [Theory]
    [InlineData(0, 1, 20)]
    [InlineData(-1, 1, 20)]
    [InlineData(1, 0, 20)]
    [InlineData(1, -1, 20)]
    [InlineData(1, 1, 0)]
    [InlineData(1, 1, -1)]
    [InlineData(1, 1, 101)]
    public async Task GetDepartmentEmployees_InvalidInput_Returns400(short departmentId, int page, int pageSize)
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync(
            $"/api/v1.0/departments/{departmentId}/employees?page={page}&pageSize={pageSize}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDepartmentEmployees_MissingDepartment_Returns404()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1.0/departments/31000/employees?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDepartmentEmployees_WithSeededDepartment_Returns200AndTotalCountHeader()
    {
        var departmentId = await SeedDepartmentWithActiveEmployeeAsync();
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/v1.0/departments/{departmentId}/employees?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("X-Total-Count", out var totalCountHeader).Should().BeTrue();
        totalCountHeader!.Single().Should().Be("1");

        var employees = await DeserializeAsync<List<EmployeeModel>>(response);
        employees.Should().NotBeNull();
        employees!.Should().ContainSingle();
    }

    private async Task<short> SeedDepartmentWithActiveEmployeeAsync()
    {
        var departmentId = (short)Random.Shared.Next(2000, 30000);
        var businessEntityId = Random.Shared.Next(200000, 500000);
        var now = DateTime.UtcNow;

        await SeedAsync(async context =>
        {
            if (!await context.PersonTypes.AnyAsync(pt => pt.PersonTypeId == 1))
            {
                context.PersonTypes.Add(new PersonTypeEntity
                {
                    PersonTypeId = 1,
                    PersonTypeCode = "EM",
                    PersonTypeName = "Employee",
                    PersonTypeDescription = "Employee"
                });
            }

            if (!await context.Shifts.AnyAsync(s => s.ShiftId == 1))
            {
                context.Shifts.Add(new ShiftEntity
                {
                    ShiftId = 1,
                    Name = "Day",
                    StartTime = TimeSpan.FromHours(8),
                    EndTime = TimeSpan.FromHours(17),
                    ModifiedDate = now
                });
            }

            context.Departments.Add(new DepartmentEntity
            {
                DepartmentId = departmentId,
                Name = $"Integration Dept {departmentId}",
                GroupName = "Integration",
                ModifiedDate = now
            });

            context.BusinessEntities.Add(new BusinessEntity
            {
                BusinessEntityId = businessEntityId,
                Rowguid = Guid.NewGuid(),
                IsEntraUser = false,
                ModifiedDate = now
            });

            context.Persons.Add(new PersonEntity
            {
                BusinessEntityId = businessEntityId,
                PersonTypeId = 1,
                NameStyle = false,
                Title = string.Empty,
                FirstName = "Dept",
                MiddleName = string.Empty,
                LastName = "Employee",
                Suffix = string.Empty,
                EmailPromotion = 0,
                AdditionalContactInfo = string.Empty,
                Demographics = string.Empty,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = now
            });

            context.Employees.Add(new EmployeeEntity
            {
                BusinessEntityId = businessEntityId,
                NationalIdnumber = $"NID{businessEntityId}",
                LoginId = $"login{businessEntityId}",
                JobTitle = "Engineer",
                BirthDate = now.AddYears(-30),
                MaritalStatus = "S",
                Gender = "F",
                HireDate = now.AddYears(-2),
                SalariedFlag = true,
                VacationHours = 20,
                SickLeaveHours = 10,
                CurrentFlag = true,
                Rowguid = Guid.NewGuid(),
                ModifiedDate = now
            });

            context.EmployeeDepartmentHistories.Add(new EmployeeDepartmentHistory
            {
                BusinessEntityId = businessEntityId,
                DepartmentId = departmentId,
                ShiftId = 1,
                StartDate = now.AddMonths(-6),
                EndDate = null,
                ModifiedDate = now
            });

            await Task.CompletedTask;
        });

        return departmentId;
    }
}
