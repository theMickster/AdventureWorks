﻿using AdventureWorks.Domain.Entities;

namespace AdventureWorks.Application.PersistenceContracts.Repositories;

public interface ICountryRegionRepository : IReadOnlyAsyncRepository<CountryRegionEntity>
{
    /// <summary>
    /// Retrieve a country region entity using its unique code
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<CountryRegionEntity> GetByIdAsync(string id);
}