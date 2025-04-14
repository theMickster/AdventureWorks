using AdventureWorks.Application.Features.AddressManagement.Contracts;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.Application.Features.AddressManagement.Services.Address;

[ServiceLifetimeScoped]
public sealed class UpdateAddressService : IUpdateAddressService
{
    private readonly IMapper _mapper;
    private readonly IAddressRepository _addressRepository;
    private readonly IValidator<AddressUpdateModel?> _validator;

    public UpdateAddressService(
        IMapper mapper,
        IAddressRepository addressRepository,
        IValidator<AddressUpdateModel?> validator
        )
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Performs business process of updating an address record.
    /// </summary>
    /// <param name="inputModel">the new address to update</param>
    /// <returns></returns>
    public async Task<(AddressModel, List<ValidationFailure>)> UpdateAsync(AddressUpdateModel inputModel)
    {
        if (inputModel == null)
        {
            throw new ArgumentNullException(nameof(inputModel));
        }

        var validationResult = await _validator.ValidateAsync(inputModel).ConfigureAwait(false);

        if (validationResult.Errors.Any())
        {
            return (new AddressModel(), validationResult.Errors);
        }
        
        var currentEntity = await _addressRepository.GetByIdAsync(inputModel.Id).ConfigureAwait(false);

        _mapper.Map(inputModel, currentEntity);
        
        await _addressRepository.UpdateAsync(currentEntity).ConfigureAwait(false);
        
        var outputModel = _mapper.Map<AddressModel>(currentEntity);

        return outputModel != null && outputModel.Id != int.MinValue && outputModel.Id != 0 ?
            (outputModel, new List<ValidationFailure>()) :
            (new AddressModel(), new List<ValidationFailure> { new("Model", "Unable to retrieve the updated model.") });

    }
}