# Testing Guide

This guide contains complete test templates for the AdventureWorks API project.

## Test Structure

```
tests/
├── AdventureWorks.UnitTests/         # xUnit unit tests
│   ├── Application/
│   │   ├── Features/                 # Handler tests (mocked repos)
│   │   │   ├── Sales/Commands/       # Command handler tests
│   │   │   ├── Sales/Queries/        # Query handler tests
│   │   │   └── Sales/Validators/     # Validator tests
│   │   └── Exceptions/
│   ├── API/
│   │   └── Controllers/v1/           # Controller tests (mocked MediatR)
│   ├── Domain/
│   │   └── Profiles/                 # AutoMapper profile tests
│   ├── Persistence/
│   │   └── Repositories/             # Repository tests (in-memory DB)
│   └── Common/                       # Utility/helper tests
│
└── AdventureWorks.Test.Common/       # Shared test utilities
    ├── Extensions/                   # Test helper extensions
    └── Setup/Fakes/                  # Mock/fake implementations
```

---

## Handler Test Template

Use this template for testing command and query handlers.

```csharp
public sealed class CreateStoreCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<IStoreRepository> _mockStoreRepository = new();
    private readonly Mock<IValidator<StoreCreateModel>> _mockValidator = new();
    private CreateStoreCommandHandler _sut;

    public CreateStoreCommandHandlerTests()
    {
        // Arrange - Setup AutoMapper
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(StoreCreateModelToStoreEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateStoreCommandHandler(_mapper, _mockStoreRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_success()
    {
        // Arrange
        var command = new CreateStoreCommand
        {
            Model = new StoreCreateModel { Name = "Contoso Store", SalesPersonId = 42 },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<StoreCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockStoreRepository.Setup(x => x.AddAsync(It.IsAny<StoreEntity>()))
            .ReturnsAsync(new StoreEntity { BusinessEntityId = 1234, Name = "Contoso Store" });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1234);
    }

    [Fact]
    public async Task Handle_throws_validation_exception()
    {
        // Arrange
        var command = new CreateStoreCommand
        {
            Model = new StoreCreateModel { Name = "   ", SalesPersonId = 123 }, // Invalid
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        var validator = new FakeFailureValidator<StoreCreateModel>("Name", "Store name is required");
        _sut = new CreateStoreCommandHandler(_mapper, _mockStoreRepository.Object, validator);

        // Act
        var act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        var exceptionAssertion = await act.Should().ThrowAsync<ValidationException>();
        exceptionAssertion.Which.Errors.Count(x => x.ErrorMessage == "Store name is required").Should().Be(1);
    }
}
```

### Key Points for Handler Tests

- Inherit from `UnitTestBase` for common test fixtures
- Use `Mock<T>` for repository and validator dependencies
- Configure AutoMapper with actual profiles for realistic mapping
- Test both success and validation failure scenarios
- Use `FakeFailureValidator<T>` for triggering validation errors

---

## Controller Test Template

Use this template for testing API controllers.

```csharp
public sealed class CreateStoreControllerTests
{
    private readonly Mock<IMediator> _mockMediator = new();
    private readonly Mock<ILogger<CreateStoreController>> _mockLogger = new();
    private CreateStoreController _sut;

    public CreateStoreControllerTests()
    {
        _sut = new CreateStoreController(_mockLogger.Object, _mockMediator.Object);
    }

    [Fact]
    public async Task PostAsync_returns_created_result()
    {
        // Arrange
        var inputModel = new StoreCreateModel { Name = "Test Store", SalesPersonId = 1 };
        var expectedModel = new StoreModel { Id = 123, Name = "Test Store", SalesPersonId = 1 };

        _mockMediator.Setup(x => x.Send(It.IsAny<CreateStoreCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(123);

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);

        // Act
        var result = await _sut.PostAsync(inputModel);

        // Assert
        result.Should().BeOfType<CreatedAtRouteResult>();
        var createdResult = (CreatedAtRouteResult)result;
        createdResult.Value.Should().BeEquivalentTo(expectedModel);
    }

    [Fact]
    public async Task PostAsync_returns_bad_request_when_model_null()
    {
        // Act
        var result = await _sut.PostAsync(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAsync_returns_ok_result()
    {
        // Arrange
        var expectedModel = new StoreModel { Id = 123, Name = "Test Store", SalesPersonId = 1 };

        _mockMediator.Setup(x => x.Send(It.IsAny<ReadStoreQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedModel);

        // Act
        var result = await _sut.GetAsync(123);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedModel);
    }
}
```

### Key Points for Controller Tests

- Mock `IMediator` to control command/query responses
- Mock `ILogger<T>` (usually not verified, just provided)
- Test expected HTTP result types (`CreatedAtRouteResult`, `OkObjectResult`, `BadRequestObjectResult`)
- Verify response values match expected models

---

## Validator Test Template

Use this template for testing FluentValidation validators.

```csharp
public sealed class CreateStoreValidatorTests
{
    private readonly CreateStoreValidator _sut = new();

    [Fact]
    public async Task Validation_succeeds_with_valid_model()
    {
        // Arrange
        var model = new StoreCreateModel { Name = "Valid Store Name", SalesPersonId = 1 };

        // Act
        var result = await _sut.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validation_fails_when_name_empty(string invalidName)
    {
        // Arrange
        var model = new StoreCreateModel { Name = invalidName, SalesPersonId = 1 };

        // Act
        var result = await _sut.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-01");
    }

    [Fact]
    public async Task Validation_fails_when_name_exceeds_max_length()
    {
        // Arrange
        var model = new StoreCreateModel
        {
            Name = new string('A', 61), // Exceeds 60 char limit
            SalesPersonId = 1
        };

        // Act
        var result = await _sut.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorCode == "Rule-02");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validation_fails_when_id_invalid(int invalidId)
    {
        // Arrange
        var model = new StoreUpdateModel { Id = invalidId, Name = "Valid Name" };
        var validator = new UpdateStoreValidator();

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Id");
    }
}
```

### Key Points for Validator Tests

- Instantiate validators directly (no mocking needed)
- Use `[Theory]` with `[InlineData]` for testing multiple invalid inputs
- Verify `IsValid` and check for specific error codes
- Test boundary conditions (max length, zero/negative IDs)

---

## Running Tests

```bash
# Navigate to solution directory
cd apps/api-dotnet

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/AdventureWorks.UnitTests/AdventureWorks.UnitTests.csproj

# Run tests matching filter
dotnet test --filter "FullyQualifiedName~CreateStore"

# Run tests in parallel (default)
dotnet test --parallel
```

---

## Test Environment

| Component | Tool/Library | Purpose |
|-----------|--------------|---------|
| Test Framework | xUnit | `[Fact]` and `[Theory]` attributes |
| Mocking | Moq 4.x | Repository/service mocks |
| Assertions | FluentAssertions | Readable test assertions |
| In-Memory DB | EF Core In-Memory | Repository integration tests |
| Test Fixtures | `UnitTestBase` | Common test data (`DefaultAuditDate`) |

**Code Coverage Target**: 80%+ for handlers, validators, and critical business logic
