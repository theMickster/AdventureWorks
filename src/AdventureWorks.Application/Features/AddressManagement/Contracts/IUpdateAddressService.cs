using AdventureWorks.Models.Features.AddressManagement;
using FluentValidation.Results;

namespace AdventureWorks.Application.Features.AddressManagement.Contracts;

public interface IUpdateAddressService
{
    /// <summary>
    /// Performs business process of updating an address record.
    /// </summary>
    /// <param name="inputModel">the new address to update</param>
    /// <returns></returns>
    Task<(AddressModel, List<ValidationFailure>)> UpdateAsync(AddressUpdateModel inputModel);
}