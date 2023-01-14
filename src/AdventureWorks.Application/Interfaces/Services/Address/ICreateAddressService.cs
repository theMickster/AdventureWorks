using AdventureWorks.Domain.Models;
using FluentValidation.Results;

namespace AdventureWorks.Application.Interfaces.Services.Address;

public interface ICreateAddressService
{

    /// <summary>
    /// Performs business process of creating a new address.
    /// </summary>
    /// <param name="inputModel">the new address to create</param>
    /// <returns></returns>
    Task<(AddressModel, List<ValidationFailure>)> CreateAsync(AddressCreateModel inputModel);

}