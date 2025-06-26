using AdventureWorks.Application.Features.Sales.Commands;
using AdventureWorks.Application.Features.Sales.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Sales.Commands;

public sealed class CreateSalesPersonCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ISalesPersonRepository> _mockSalesPersonRepository = new();
    private readonly Mock<IValidator<SalesPersonCreateModel>> _mockValidator = new();
    private CreateSalesPersonCommandHandler _sut;

    public CreateSalesPersonCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(SalesPersonCreateModelToSalesPersonEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateSalesPersonCommandHandler(_mapper, _mockSalesPersonRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public void constructor_throws_correct_exceptions()
    {
        _sut.GetType().ConstructorNullExceptions();
        Assert.True(true);
    }

    [Fact]
    public void Handle_throws_correct_exception()
    {
        ((Func<Task>)(async () => await _sut.Handle(null!, CancellationToken.None))).Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Handle_throws_correct_validation_errorsAsync()
    {
        var command = new CreateSalesPersonCommand
        {
            Model = new SalesPersonCreateModel
            {
                BusinessEntityId = 0,
                CommissionPct = 0.05m,
                Bonus = 1000,
                TerritoryId = 1
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        var validator = new FakeFailureValidator<SalesPersonCreateModel>("BusinessEntityId", "Business Entity ID is required");

        _sut = new CreateSalesPersonCommandHandler(_mapper, _mockSalesPersonRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Business Entity ID is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new CreateSalesPersonCommand
        {
            Model = new SalesPersonCreateModel
            {
                BusinessEntityId = 100,
                CommissionPct = 0.05m,
                Bonus = 1000,
                TerritoryId = 1,
                SalesQuota = 250000
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = new Guid("5ec92f1e-232b-430e-a729-ea59c943e3fc")
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<SalesPersonCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockSalesPersonRepository.Setup(x => x.AddAsync(It.IsAny<SalesPersonEntity>()))
            .ReturnsAsync(new SalesPersonEntity
            {
                BusinessEntityId = 100,
                CommissionPct = 0.05m,
                Bonus = 1000,
                TerritoryId = 1,
                SalesQuota = 250000,
                ModifiedDate = DefaultAuditDate,
                Rowguid = command.RowGuid
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(100);
    }
}
