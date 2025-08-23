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
public sealed class UpdateProductCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IProductRepository> _mockProductRepository = new();
    private readonly Mock<IValidator<ProductUpdateModel>> _mockValidator = new();
    private UpdateProductCommandHandler _sut;

    public UpdateProductCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c => c.AddMaps(typeof(ProductUpdateModelToProductProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new UpdateProductCommandHandler(_mapper, _mockProductRepository.Object, _mockValidator.Object);
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
        var command = new UpdateProductCommand
        {
            Model = new ProductUpdateModel
            {
                Id = 1,
                Name = "       ",
                ProductNumber = "TP-001",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 10m,
                ListPrice = 25m,
                SellStartDate = new DateTime(2024, 1, 1)
            },
            ModifiedDate = DefaultAuditDate
        };

        var validator = new FakeFailureValidator<ProductUpdateModel>("Name", "Product name is required");

        _sut = new UpdateProductCommandHandler(_mapper, _mockProductRepository.Object, validator);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Product name is required").Should().Be(1);
    }

    [Fact]
    public async Task Handle_throws_when_entity_not_foundAsync()
    {
        var command = new UpdateProductCommand
        {
            Model = new ProductUpdateModel
            {
                Id = 999,
                Name = "Test",
                ProductNumber = "TP-001",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 10m,
                ListPrice = 25m,
                SellStartDate = new DateTime(2024, 1, 1)
            },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ProductUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockProductRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var act = async () => await _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Product with ID 999 not found.");
    }

    [Fact]
    public async Task Handle_succeeds_when_validAsync()
    {
        var command = new UpdateProductCommand
        {
            Model = new ProductUpdateModel
            {
                Id = 1,
                Name = "Updated Product",
                ProductNumber = "TP-001",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 10m,
                ListPrice = 25m,
                SellStartDate = new DateTime(2024, 1, 1)
            },
            ModifiedDate = DefaultAuditDate
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<ProductUpdateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult { Errors = [] });

        _mockProductRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                ProductId = 1,
                Name = "Old Product",
                ProductNumber = "TP-001",
                SafetyStockLevel = 100,
                ReorderPoint = 75,
                StandardCost = 10m,
                ListPrice = 25m,
                SellStartDate = new DateTime(2024, 1, 1),
                ModifiedDate = DefaultAuditDate,
                Rowguid = Guid.NewGuid()
            });

        _mockProductRepository.Setup(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(command, CancellationToken.None);

        _mockProductRepository.Verify(x => x.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
