using AdventureWorks.Models.Features.AddressManagement;
using FluentValidation.Results;

namespace AdventureWorks.Application.Features.AddressManagement.Contracts;

public interface ICreateAddressService
{

    /// <summary>
    /// Performs business process of creating a new address.
    /// </summary>
    /// <param name="inputModel">the new address to create</param>
    /// <returns></returns>
    Task<(AddressModel, List<ValidationFailure>)> CreateAsync(AddressCreateModel inputModel);

}