using AdventureWorks.Domain.Models;
using FluentValidation.Results;

namespace AdventureWorks.Application.Interfaces.Services.Address;

public interface IUpdateAddressService
{
    /// <summary>
    /// Performs business process of updating an address record.
    /// </summary>
    /// <param name="inputModel">the new address to update</param>
    /// <returns></returns>
    Task<(AddressModel, List<ValidationFailure>)> UpdateAsync(AddressUpdateModel inputModel);
}