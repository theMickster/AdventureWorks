using AdventureWorks.Application.Features.Production.Commands;
using AdventureWorks.Application.Features.Production.Profiles;
using AdventureWorks.Application.PersistenceContracts.Repositories.Production;
using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AdventureWorks.Test.Common.Extensions;
using AdventureWorks.UnitTests.Setup.Fakes;
using FluentValidation;

namespace AdventureWorks.UnitTests.Application.Features.Production.Commands;

[ExcludeFromCodeCoverage]
public sealed class CreateProductCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly Mock<IValidator<ProductCreateModel>> _mockValidator = new();
    private CreateProductCommandHandler _sut;

    public CreateProductCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductCreateModelToProductProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateProductCommandHandler(_mapper, _mockProductRepository.Object, _mockValidator.Object);
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
        var command = new CreateProductCommand
        {
            Model = new ProductCreateModel
            {
                Name = "       ",
                ProductNumber = "TP-001",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 10m,
                ListPrice = 25m,
                SellStartDate = new DateTime(2024, 1, 1)
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        var validator = new FakeFailureValidator<ProductCreateModel>("Name", "Product name is required");

        _sut = new CreateProductCommandHandler(_mapper, _mockProductRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Product name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_returns_successAsync()
    {
        var command = new CreateProductCommand
        {
            Model = new ProductCreateModel
            {
                Name = "Test Product",
                ProductNumber = "TP-001",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 10m,
                ListPrice = 25m,
                SellStartDate = new DateTime(2024, 1, 1)
            },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ProductCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockProductRepository.Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                ProductId = 999,
                Name = "Test Product",
                ProductNumber = "TP-001",
                ModifiedDate = DefaultAuditDate,
                Rowguid = command.RowGuid
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Should().Be(999);
    }
}
