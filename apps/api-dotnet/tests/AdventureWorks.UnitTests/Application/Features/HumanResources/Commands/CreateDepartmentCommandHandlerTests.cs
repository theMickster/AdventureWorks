using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class CreateDepartmentCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository = new();
    private readonly Mock<IValidator<DepartmentCreateModel>> _mockValidator = new();
    private CreateDepartmentCommandHandler _sut;

    public CreateDepartmentCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<DepartmentCreateModelToDepartmentEntityProfile>();
        });
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateDepartmentCommandHandler(_mapper, _mockDepartmentRepository.Object, _mockValidator.Object);
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
    public async Task Handle_throws_exception_when_request_model_is_nullAsync()
    {
        var command = new CreateDepartmentCommand
        {
            Model = null!,
            ModifiedDate = DefaultAuditDate
        };

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("request.Model");
    }

    [Fact]
    public async Task Handle_throws_validation_exception_when_model_is_invalidAsync()
    {
        var command = new CreateDepartmentCommand
        {
            Model = new DepartmentCreateModel { Name = "", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<DepartmentCreateModel>(
            "Name",
            "Department name is required");

        _sut = new CreateDepartmentCommandHandler(_mapper, _mockDepartmentRepository.Object, validator);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "Department name is required")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_department_id_when_successfulAsync()
    {
        var command = new CreateDepartmentCommand
        {
            Model = new DepartmentCreateModel { Name = "Engineering", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DepartmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        const short expectedDepartmentId = 42;

        _mockDepartmentRepository
            .Setup(x => x.AddAsync(It.IsAny<DepartmentEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = expectedDepartmentId });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(expectedDepartmentId);
    }

    [Fact]
    public async Task Handle_sets_modified_date_on_entityAsync()
    {
        var command = new CreateDepartmentCommand
        {
            Model = new DepartmentCreateModel { Name = "Engineering", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DepartmentCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        DepartmentEntity? capturedEntity = null;

        _mockDepartmentRepository
            .Setup(x => x.AddAsync(It.IsAny<DepartmentEntity>(), It.IsAny<CancellationToken>()))
            .Callback<DepartmentEntity, CancellationToken>((entity, _) => capturedEntity = entity)
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 1 });

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            capturedEntity.Should().NotBeNull();
            capturedEntity!.ModifiedDate.Should().Be(DefaultAuditDate);
            capturedEntity.Name.Should().Be("Engineering");
            capturedEntity.GroupName.Should().Be("Research and Development");
        }
    }
}
