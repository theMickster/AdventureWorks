using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace AdventureWorks.Application.Features.AddressManagement.Commands;

public sealed class CreateAddressCommandHandler (
    IMapper mapper,
    IAddressRepository addressRepository,
    IValidator<AddressCreateModel> validator) 
        : IRequestHandler<CreateAddressCommand, int>
{
    private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IAddressRepository _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
    private readonly IValidator<AddressCreateModel> _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    
    public async Task<int> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        await _validator.ValidateAndThrowAsync(request.Model, cancellationToken);
        
        var inputEntity = _mapper.Map<AddressEntity>(request.Model);
        inputEntity.ModifiedDate = request.ModifiedDate;
        inputEntity.Rowguid = request.RowGuid;

        var outputEntity = await _addressRepository.AddAsync(inputEntity);
        
        return outputEntity.AddressId;
    }
}
