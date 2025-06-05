using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Commands;

public sealed class UpdateAddressCommandHandler(
    IMapper mapper,
    IAddressRepository addressRepository,
    IValidator<AddressUpdateModel?> validator) 
        : IRequestHandler<UpdateAddressCommand>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IAddressRepository _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
    private readonly IValidator<AddressUpdateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);
        var currentEntity = await _addressRepository.GetByIdAsync(request.Model.Id);
        _mapper.Map(request.Model, currentEntity);
        currentEntity.ModifiedDate = request.ModifiedDate;
        await _addressRepository.UpdateAsync(currentEntity);
    }
}
