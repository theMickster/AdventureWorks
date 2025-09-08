using AdventureWorks.Application.Features.HumanResources.Queries;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Queries;

[ExcludeFromCodeCoverage]
public sealed class ReadEmployeeOrgTreeQueryHandlerTests : UnitTestBase
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository = new();
    private readonly ReadEmployeeOrgTreeQueryHandler _sut;

    public ReadEmployeeOrgTreeQueryHandlerTests()
    {
        _sut = new ReadEmployeeOrgTreeQueryHandler(_mockEmployeeRepository.Object);
    }

    [Fact]
    public void Constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_exception_when_request_is_null()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None)))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_returns_empty_list_when_no_employeesAsync()
    {
        _mockEmployeeRepository
            .Setup(x => x.GetOrgTreeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EmployeeOrgTreeItemModel>().AsReadOnly());

        var result = await _sut.Handle(new ReadEmployeeOrgTreeQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_returns_org_tree_when_employees_existAsync()
    {
        var orgTree = new List<EmployeeOrgTreeItemModel>
        {
            new()
            {
                EmployeeId = 1,
                FullName = "John Doe",
                JobTitle = "CEO",
                DepartmentName = "Executive",
                OrganizationLevel = 1,
                ParentEmployeeId = null
            },
            new()
            {
                EmployeeId = 2,
                FullName = "Jane Smith",
                JobTitle = "VP Engineering",
                DepartmentName = "Engineering",
                OrganizationLevel = 2,
                ParentEmployeeId = 1
            }
        };

        _mockEmployeeRepository
            .Setup(x => x.GetOrgTreeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orgTree.AsReadOnly());

        var result = await _sut.Handle(new ReadEmployeeOrgTreeQuery(), CancellationToken.None);

        using (new AssertionScope())
        {
            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result[0].EmployeeId.Should().Be(1);
            result[0].ParentEmployeeId.Should().BeNull();
            result[1].EmployeeId.Should().Be(2);
            result[1].ParentEmployeeId.Should().Be(1);
        }
    }
}
