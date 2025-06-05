using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.UnitTests.Setup.Fakes;

internal sealed class FakeFailureValidator<T>(
    string propertyName = "FakeProperty",
    string errorMessage = "Fake validation error")
    : IValidator<T>
{
    public ValidationResult Validate(T instance)
    {
        var failure = new ValidationFailure(propertyName, errorMessage);
        throw new ValidationException(new List<ValidationFailure> { failure });
    }

    public ValidationResult Validate(ValidationContext<T> context)
    {
        var failure = new ValidationFailure(propertyName, errorMessage);
        throw new ValidationException(new List<ValidationFailure> { failure });
    }

    public ValidationResult Validate(IValidationContext context)
    {
        var failure = new ValidationFailure(propertyName, errorMessage);
        throw new ValidationException(new List<ValidationFailure> { failure });
    }

    public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = default)
    {
        var failure = new ValidationFailure(propertyName, errorMessage);
        throw new ValidationException(new List<ValidationFailure> { failure });
    }

    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default)
    {
        var failure = new ValidationFailure(propertyName, errorMessage);
        throw new ValidationException(new List<ValidationFailure> { failure });
    }

    public Task<ValidationResult> ValidateAsync(ValidationContext<T> context, CancellationToken cancellation = default)
    {
        var failure = new ValidationFailure(propertyName, errorMessage);
        throw new ValidationException(new List<ValidationFailure> { failure });
    }

    public IValidatorDescriptor CreateDescriptor() => throw new NotImplementedException();

    public bool CanValidateInstancesOfType(Type type) => throw new NotImplementedException();
}