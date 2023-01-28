﻿using AdventureWorks.Domain.Models;

namespace AdventureWorks.Application.Interfaces.Services.SalesTerritory;

public interface IReadSalesTerritoryService
{
    /// <summary>
    /// Retrieve a sales territory using its identifier.
    /// </summary>
    /// <returns>A <see cref="SalesTerritoryModel"/> </returns>
    Task<SalesTerritoryModel?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieve the list of sales territories. 
    /// </summary>
    /// <returns></returns>
    Task<List<SalesTerritoryModel>> GetListAsync();
}