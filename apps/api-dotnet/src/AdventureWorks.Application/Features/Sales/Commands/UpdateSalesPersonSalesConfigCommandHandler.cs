using AdventureWorks.Application.Features.Dashboard.Notifications;
using AdventureWorks.Application.PersistenceContracts.Repositories.Sales;
using AdventureWorks.Models.Features.Sales;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.Sales.Commands;

/// <summary>
/// Handles <see cref="UpdateSalesPersonSalesConfigCommand"/> by patching the four sales config fields on the existing entity and publishing an <c>EntityChangedNotification</c>.
/// </summary>
public sealed class UpdateSalesPersonSalesConfigCommandHandler(
    ISalesPersonRepository salesPersonRepository,
    IValidator<SalesPersonSalesConfigUpdateModel> validator,
    IPublisher publisher)
        : IRequestHandler<UpdateSalesPersonSalesConfigCommand>
{
    private readonly ISalesPersonRepository _salesPersonRepository = salesPersonRepository ?? throw new ArgumentNullException(nameof(salesPersonRepository));
    private readonly IValidator<SalesPersonSalesConfigUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    private readonly IPublisher _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));

    /// <summary>
    /// Validates the request, loads the sales person entity, patches the four config fields, persists via the cascade update, and publishes a change notification.
    /// </summary>
    public async Task Handle(UpdateSalesPersonSalesConfigCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);

        var entity = await _salesPersonRepository.GetSalesPersonByIdAsync(request.Model.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Sales person with id {request.Model.Id} was not found.");
        }

        // DB referential integrity guarantees these nav-props are populated for any valid SalesPersonEntity.
        ArgumentNullException.ThrowIfNull(entity.Employee);
        ArgumentNullException.ThrowIfNull(entity.Employee.PersonBusinessEntity);

        entity.TerritoryId = request.Model.TerritoryId;
        entity.SalesQuota = request.Model.SalesQuota;
        entity.Bonus = request.Model.Bonus;
        entity.CommissionPct = request.Model.CommissionPct;

        // Prevents EF identity conflict: validator's GetByIdAsync already tracked this entity.
        entity.SalesTerritory = null;

        await _salesPersonRepository.UpdateSalesPersonWithEmployeeAsync(
            entity,
            entity.Employee,
            entity.Employee.PersonBusinessEntity,
            request.ModifiedDate,
            cancellationToken);

        await _publisher.Publish(new EntityChangedNotification
        {
            EntityType = "SalesPerson",
            EntityId = request.Model.Id,
            Action = "Updated",
            UserName = request.UserName,
            Timestamp = request.ModifiedDate
        }, cancellationToken);
    }
}
