using AdventureWorks.Domain.Entities.Sales;
using AdventureWorks.Models.Features.Sales;
using AutoMapper;

namespace AdventureWorks.Application.Features.Sales.Profiles;

/// <summary>
/// AutoMapper profile for mapping SalesOrderHeader entity to SalesOrderModel DTO.
/// </summary>
public sealed class SalesOrderEntityToModelProfile : Profile
{
    public SalesOrderEntityToModelProfile()
    {
        CreateMap<SalesOrderHeader, SalesOrderModel>()
            .ForMember(dest => dest.StatusDescription, opt => opt.MapFrom<StatusDescriptionResolver>())
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom<CustomerNameResolver>())
            .ForMember(dest => dest.SalesPersonName, opt => opt.MapFrom<SalesPersonNameResolver>());
    }
}

/// <summary>
/// Custom resolver for status description.
/// </summary>
public sealed class StatusDescriptionResolver : IValueResolver<SalesOrderHeader, SalesOrderModel, string>
{
    /// <summary>
    /// Resolves the human-readable description for a sales order status code.
    /// </summary>
    /// <param name="src">the source sales order entity</param>
    /// <param name="dest">the destination sales order model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>The status description (e.g., "In process", "Shipped")</returns>
    public string Resolve(SalesOrderHeader src, SalesOrderModel dest, string destMember, ResolutionContext context)
    {
        return src.Status switch
        {
            1 => "In process",
            2 => "Approved",
            3 => "Backordered",
            4 => "Rejected",
            5 => "Shipped",
            6 => "Cancelled",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Custom resolver for customer name with null-safety.
/// </summary>
public sealed class CustomerNameResolver : IValueResolver<SalesOrderHeader, SalesOrderModel, string>
{
    /// <summary>
    /// Resolves the customer's full name from the sales order entity.
    /// </summary>
    /// <param name="src">the source sales order entity</param>
    /// <param name="dest">the destination sales order model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>The customer display name: "FirstName LastName" for individuals, the store name for store customers, or an empty string if neither is available.</returns>
    public string Resolve(SalesOrderHeader src, SalesOrderModel dest, string destMember, ResolutionContext context)
    {
        var customer = src.CustomerEntity;
        if (customer is null)
        {
            return string.Empty;
        }

        if (customer.Person is not null)
        {
            var firstName = customer.Person.FirstName ?? string.Empty;
            var lastName = customer.Person.LastName ?? string.Empty;
            return $"{firstName} {lastName}".Trim();
        }

        return customer.StoreEntity?.Name ?? string.Empty;
    }
}

/// <summary>
/// Custom resolver for sales person name.
/// </summary>
public sealed class SalesPersonNameResolver : IValueResolver<SalesOrderHeader, SalesOrderModel, string?>
{
    /// <summary>
    /// Resolves the sales person's full name from the sales order entity.
    /// </summary>
    /// <param name="src">the source sales order entity</param>
    /// <param name="dest">the destination sales order model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>The sales person's full name (FirstName LastName), or null if no sales person is assigned</returns>
    public string? Resolve(SalesOrderHeader src, SalesOrderModel dest, string? destMember, ResolutionContext context)
    {
        if (src.SalesPerson?.Employee?.PersonBusinessEntity == null)
        {
            return null;
        }

        var person = src.SalesPerson.Employee.PersonBusinessEntity;
        return $"{person.FirstName} {person.LastName}".Trim();
    }
}
