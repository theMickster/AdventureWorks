# Adding New Feature (CQRS Command/Query)

Step-by-step checklist for adding a new CRUD feature. Example: Creating a new "Customer" feature.

## 1. Define the Entity

**Location**: `src/AdventureWorks.Domain/Entities/Sales/CustomerEntity.cs`

```csharp
namespace AdventureWorks.Domain.Entities.Sales;

public sealed class CustomerEntity : BaseEntity
{
    public int CustomerId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime ModifiedDate { get; set; }
    public Guid Rowguid { get; set; }
}
```

## 2. Define the DTOs

**Location**: `src/AdventureWorks.Models/Features/Sales/`

```csharp
// CustomerModel.cs - Read DTO
public sealed class CustomerModel
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
}

// CustomerCreateModel.cs - Create DTO
public sealed class CustomerCreateModel : CustomerBaseModel
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

// CustomerBaseModel.cs - Shared validation base
public abstract class CustomerBaseModel
{
    // Common properties
}
```

## 3. Create Repository Interface

**Location**: `src/AdventureWorks.Application/PersistenceContracts/Repositories/Sales/ICustomerRepository.cs`

```csharp
namespace AdventureWorks.Application.PersistenceContracts.Repositories.Sales;

public interface ICustomerRepository : IAsyncRepository<CustomerEntity>
{
    // Add custom query methods if needed
    Task<CustomerEntity?> GetByEmailAsync(string email);
}
```

## 4. Implement Repository

**Location**: `src/AdventureWorks.Infrastructure.Persistence/Repositories/Sales/CustomerRepository.cs`

```csharp
public sealed class CustomerRepository(AdventureWorksDbContext dbContext)
    : EfRepository<CustomerEntity>(dbContext), ICustomerRepository
{
    public async Task<CustomerEntity?> GetByEmailAsync(string email)
    {
        return await DbContext.Set<CustomerEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Email == email);
    }
}
```

## 5. Create Validator

**Location**: `src/AdventureWorks.Application/Features/Sales/Validators/`

```csharp
// CustomerBaseModelValidator.cs
public class CustomerBaseModelValidator<T> : AbstractValidator<T> where T : CustomerBaseModel
{
    public CustomerBaseModelValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("Rule-01")
            .WithMessage("Customer name is required")
            .MaximumLength(100).WithErrorCode("Rule-02")
            .WithMessage("Customer name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode("Rule-03")
            .EmailAddress().WithErrorCode("Rule-04")
            .WithMessage("Valid email address is required");
    }
}

// CreateCustomerValidator.cs
public sealed class CreateCustomerValidator : CustomerBaseModelValidator<CustomerCreateModel>
{
    public CreateCustomerValidator() { }
}
```

## 6. Create AutoMapper Profiles

**Location**: `src/AdventureWorks.Application/Features/Sales/Profiles/`

```csharp
// CustomerCreateModelToCustomerEntityProfile.cs
public sealed class CustomerCreateModelToCustomerEntityProfile : Profile
{
    public CustomerCreateModelToCustomerEntityProfile()
    {
        CreateMap<CustomerCreateModel, CustomerEntity>()
            .ForMember(x => x.CustomerId, o => o.Ignore())
            .ForMember(x => x.ModifiedDate, o => o.Ignore())
            .ForMember(x => x.Rowguid, o => o.Ignore());
    }
}

// CustomerEntityToModelProfile.cs
public sealed class CustomerEntityToModelProfile : Profile
{
    public CustomerEntityToModelProfile()
    {
        CreateMap<CustomerEntity, CustomerModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CustomerId));
    }
}
```

## 7. Implement Command

**Location**: `src/AdventureWorks.Application/Features/Sales/Commands/`

```csharp
// CreateCustomerCommand.cs
public sealed class CreateCustomerCommand : IRequest<int>
{
    public required CustomerCreateModel Model { get; init; }
    public required DateTime ModifiedDate { get; init; }
    public required Guid RowGuid { get; init; }
}

// CreateCustomerCommandHandler.cs (sealed, primary constructor)
public sealed class CreateCustomerCommandHandler(
    IMapper mapper,
    ICustomerRepository customerRepository,
    IValidator<CustomerCreateModel> validator)
        : IRequestHandler<CreateCustomerCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly ICustomerRepository _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    private readonly IValidator<CustomerCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<int> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var entity = _mapper.Map<CustomerEntity>(request.Model);
        entity.ModifiedDate = request.ModifiedDate;
        entity.Rowguid = request.RowGuid;

        var outputEntity = await _customerRepository.AddAsync(entity);
        return outputEntity.CustomerId;
    }
}
```

## 8. Implement Query

**Location**: `src/AdventureWorks.Application/Features/Sales/Queries/`

```csharp
// ReadCustomerQuery.cs
public sealed class ReadCustomerQuery : IRequest<CustomerModel>
{
    public required int Id { get; init; }
}

// ReadCustomerQueryHandler.cs
public sealed class ReadCustomerQueryHandler(
    IMapper mapper,
    ICustomerRepository customerRepository)
        : IRequestHandler<ReadCustomerQuery, CustomerModel>
{
    public async Task<CustomerModel> Handle(ReadCustomerQuery request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await customerRepository.GetByIdAsync(request.Id);
        return mapper.Map<CustomerModel>(entity);
    }
}
```

## 9. Create Controller

**Location**: `src/AdventureWorks.API/Controllers/v1/Customers/CreateCustomerController.cs`

```csharp
[ApiController]
[Authorize]  // Require authentication by default
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "Customer")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CreateCustomerController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Produces(typeof(CustomerModel))]
    public async Task<IActionResult> PostAsync([FromBody] CustomerCreateModel? inputModel)
    {
        if (inputModel == null)
        {
            return BadRequest("The customer input model cannot be null.");
        }

        var cmd = new CreateCustomerCommand
        {
            Model = inputModel,
            ModifiedDate = DateTime.UtcNow,
            RowGuid = Guid.NewGuid()
        };

        var customerId = await mediator.Send(cmd);
        var model = await mediator.Send(new ReadCustomerQuery { Id = customerId });

        return CreatedAtRoute("GetCustomerById", new { customerId = model.Id }, model);
    }
}
```

## 10. Write Tests

**Location**: `tests/AdventureWorks.UnitTests/Application/Features/Sales/Commands/`

```csharp
public sealed class CreateCustomerCommandHandlerTests : UnitTestBase
{
    private readonly IMapper _mapper;
    private readonly Mock<ICustomerRepository> _mockCustomerRepository = new();
    private readonly Mock<IValidator<CustomerCreateModel>> _mockValidator = new();
    private CreateCustomerCommandHandler _sut;

    public CreateCustomerCommandHandlerTests()
    {
        var mappingConfig = new MapperConfiguration(c =>
            c.AddMaps(typeof(CustomerCreateModelToCustomerEntityProfile).Assembly));
        _mapper = mappingConfig.CreateMapper();
        _sut = new CreateCustomerCommandHandler(_mapper, _mockCustomerRepository.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task Handle_returns_success()
    {
        // Arrange
        var command = new CreateCustomerCommand
        {
            Model = new CustomerCreateModel { Name = "Contoso", Email = "contact@contoso.com" },
            ModifiedDate = DefaultAuditDate,
            RowGuid = Guid.NewGuid()
        };

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<CustomerCreateModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult { Errors = [] });

        _mockCustomerRepository.Setup(x => x.AddAsync(It.IsAny<CustomerEntity>()))
            .ReturnsAsync(new CustomerEntity { CustomerId = 1234, Name = "Contoso", Email = "contact@contoso.com" });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(1234);
    }
}
```

## 11. Update Documentation

- Add route to "Example API Routes" section in CLAUDE.md
- Update domain areas if introducing a new domain

---

## Checklist Summary

| Step | Artifact | Location |
|------|----------|----------|
| 1 | Entity | `Domain/Entities/{Domain}/` |
| 2 | DTOs | `Models/Features/{Domain}/` |
| 3 | Repository Interface | `Application/PersistenceContracts/Repositories/{Domain}/` |
| 4 | Repository Implementation | `Infrastructure.Persistence/Repositories/{Domain}/` |
| 5 | Validator | `Application/Features/{Domain}/Validators/` |
| 6 | AutoMapper Profiles | `Application/Features/{Domain}/Profiles/` |
| 7 | Command + Handler | `Application/Features/{Domain}/Commands/` |
| 8 | Query + Handler | `Application/Features/{Domain}/Queries/` |
| 9 | Controller | `API/Controllers/v1/{Feature}/` |
| 10 | Unit Tests | `UnitTests/Application/Features/{Domain}/` |
| 11 | Documentation | Update CLAUDE.md |
