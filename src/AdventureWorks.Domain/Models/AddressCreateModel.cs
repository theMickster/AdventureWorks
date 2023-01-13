﻿namespace AdventureWorks.Domain.Models;

public sealed class AddressCreateModel
{
    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public StateProvinceModel StateProvince { get; set; }

    public string PostalCode { get; set; }
}