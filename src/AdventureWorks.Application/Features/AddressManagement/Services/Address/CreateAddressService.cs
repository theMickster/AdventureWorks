using AdventureWorks.Application.Features.AddressManagement.Contracts;
using AdventureWorks.Application.PersistenceContracts.Repositories;
using AdventureWorks.Common.Attributes;
using AdventureWorks.Domain.Entities;
using AdventureWorks.Models.Features.AddressManagement;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;

namespace AdventureWorks.Application.Features.AddressManagement.Services.Address;

[ServiceLifetimeScoped]
public sealed class CreateAddressService : ICreateAddressService
{
    private readonly IMapper _mapper;
    private readonly IAddressRepository _addressRepository;
    private readonly IValidator<AddressCreateModel?> _validator;

    public CreateAddressService(
        IMapper mapper, 
        IAddressRepository addressRepository,
        IValidator<AddressCreateModel?> validator)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _addressRepository = addressRepository ?? throw new ArgumentNullException(nameof(addressRepository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Performs business process of creating a new address.
    /// </summary>
    /// <param name="inputModel">the new address to create</param>
    /// <returns></returns>
    public async Task<(AddressModel, List<ValidationFailure>)> CreateAsync(AddressCreateModel inputModel)
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

        var inputEntity = _mapper.Map<AddressEntity>(inputModel);
        inputEntity.ModifiedDate = DateTime.UtcNow;
        inputEntity.Rowguid = Guid.NewGuid();

        var outputEntity = await _addressRepository.AddAsync(inputEntity).ConfigureAwait(false);
        var outputModel = _mapper.Map<AddressModel>(outputEntity);

        return outputModel != null && outputModel.Id != int.MinValue && outputModel.Id != 0 ?
            (outputModel, new List<ValidationFailure>()) :
            (new AddressModel(), new List<ValidationFailure> { new("Model", "Unable to retrieve the newly created model.") });
    }
}