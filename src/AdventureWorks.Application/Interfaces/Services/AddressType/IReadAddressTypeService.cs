﻿using AdventureWorks.Domain.Models;

namespace AdventureWorks.Application.Interfaces.Services.AddressType;

public interface IReadAddressTypeService
{
    /// <summary>
    /// Retrieve an address type using its identifier.
    /// </summary>
    /// <returns>A <see cref="AddressTypeModel"/> </returns>
    Task<AddressTypeModel?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve the list of address types. 
    /// </summary>
    /// <returns></returns>
    Task<List<AddressTypeModel>> GetListAsync();
}
