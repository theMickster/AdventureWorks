using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

/// <summary>
/// AutoMapper profile for mapping WorkOrder entity to WorkOrderModel DTO.
/// </summary>
public sealed class WorkOrderEntityToModelProfile : Profile
{
    public WorkOrderEntityToModelProfile()
    {
        CreateMap<WorkOrder, WorkOrderModel>()
            .ForMember(dest => dest.OrderedQty, opt => opt.MapFrom(src => src.OrderQty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom<ProductNameResolver>())
            .ForMember(dest => dest.YieldRate, opt => opt.MapFrom<YieldRateResolver>())
            .ForMember(dest => dest.IsCompletedLate, opt => opt.MapFrom<IsCompletedLateResolver>());
    }
}

/// <summary>
/// Custom resolver for the manufactured product's display name.
/// </summary>
public sealed class ProductNameResolver : IValueResolver<WorkOrder, WorkOrderModel, string>
{
    /// <summary>
    /// Resolves the product name from the work order's related product entity.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>The product name, or an empty string if the product relationship is not loaded</returns>
    public string Resolve(WorkOrder src, WorkOrderModel dest, string destMember, ResolutionContext context) =>
        src.Product?.Name ?? string.Empty;
}

/// <summary>
/// Custom resolver for the yield rate computed field.
/// </summary>
public sealed class YieldRateResolver : IValueResolver<WorkOrder, WorkOrderModel, decimal>
{
    /// <summary>
    /// Resolves the percentage of ordered units successfully stocked.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>StockedQty / OrderQty * 100, rounded to 2 decimal places, or 0 when OrderQty is 0</returns>
    public decimal Resolve(WorkOrder src, WorkOrderModel dest, decimal destMember, ResolutionContext context) =>
        src.OrderQty == 0 ? 0m : Math.Round((decimal)src.StockedQty / src.OrderQty * 100, 2);
}

/// <summary>
/// Custom resolver for the completed-late computed field.
/// </summary>
public sealed class IsCompletedLateResolver : IValueResolver<WorkOrder, WorkOrderModel, bool>
{
    /// <summary>
    /// Resolves whether the work order finished after its due date.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>True when <c>EndDate</c> is set and later than <c>DueDate</c>; otherwise false</returns>
    public bool Resolve(WorkOrder src, WorkOrderModel dest, bool destMember, ResolutionContext context) =>
        src.EndDate.HasValue && src.EndDate.Value > src.DueDate;
}
