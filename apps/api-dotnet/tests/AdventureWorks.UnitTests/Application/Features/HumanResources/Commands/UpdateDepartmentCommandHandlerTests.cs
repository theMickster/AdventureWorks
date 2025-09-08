using AdventureWorks.Application.Features.HumanResources.Commands;
using AdventureWorks.Application.Features.HumanResources.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.HumanResources;
using AdventureWorks.Domain.Entities.HumanResources;
using AdventureWorks.Models.Features.HumanResources;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace AdventureWorks.UnitTests.Application.Features.HumanResources.Commands;

[ExcludeFromCodeCoverage]
public sealed class UpdateDepartmentCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository = new();
    private readonly Mock<IValidator<DepartmentUpdateModel>> _mockValidator = new();
    private UpdateDepartmentCommandHandler _sut;

    public UpdateDepartmentCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<DepartmentUpdateModelToDepartmentEntityProfile>();
            c.AddProfile<DepartmentEntityToDepartmentModelProfile>();
        });
        _mapper = mappingConfig.CreateMapper();
        _sut = new UpdateDepartmentCommandHandler(_mapper, _mockDepartmentRepository.Object, _mockValidator.Object);
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
        var command = new UpdateDepartmentCommand
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
        var command = new UpdateDepartmentCommand
        {
            Model = new DepartmentUpdateModel { Id = 1, Name = "", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<DepartmentUpdateModel>(
            "Name",
            "Department name is required");

        _sut = new UpdateDepartmentCommandHandler(_mapper, _mockDepartmentRepository.Object, validator);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors
            .Count(x => x.ErrorMessage == "Department name is required")
            .Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_key_not_found_when_department_does_not_existAsync()
    {
        var command = new UpdateDepartmentCommand
        {
            Model = new DepartmentUpdateModel { Id = 99, Name = "Engineering", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DepartmentUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DepartmentEntity?)null);

        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Department with ID 99 not found.");
    }

    [Fact]
    public async Task Handle_returns_unit_when_successfulAsync()
    {
        var command = new UpdateDepartmentCommand
        {
            Model = new DepartmentUpdateModel { Id = 1, Name = "Engineering Updated", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DepartmentUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DepartmentEntity { DepartmentId = 1, Name = "Engineering", GroupName = "Research and Development" });

        _mockDepartmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<DepartmentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_sets_modified_date_on_entityAsync()
    {
        var command = new UpdateDepartmentCommand
        {
            Model = new DepartmentUpdateModel { Id = 1, Name = "Engineering Updated", GroupName = "Research and Development" },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<DepartmentUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        var existingEntity = new DepartmentEntity
        {
            DepartmentId = 1,
            Name = "Engineering",
            GroupName = "Research and Development",
            ModifiedDate = DateTime.UtcNow.AddDays(-30)
        };

        _mockDepartmentRepository
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity);

        _mockDepartmentRepository
            .Setup(x => x.UpdateAsync(It.IsAny<DepartmentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        using (new AssertionScope())
        {
            existingEntity.ModifiedDate.Should().Be(DefaultAuditDate);
            existingEntity.Name.Should().Be("Engineering Updated");
        }
    }
}
