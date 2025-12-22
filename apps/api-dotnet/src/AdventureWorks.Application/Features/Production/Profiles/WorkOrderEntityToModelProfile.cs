using AdventureWorks.Domain.Entities.Production;
using AdventureWorks.Models.Features.Production;
using AutoMapper;

namespace AdventureWorks.Application.Features.Production.Profiles;

/// <summary>
/// AutoMapper profile for mapping WorkOrder entity to WorkOrderModel and WorkOrderDetailModel DTOs.
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

        CreateMap<WorkOrder, WorkOrderDetailModel>()
            .ForMember(dest => dest.OrderedQty, opt => opt.MapFrom(src => src.OrderQty))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom<ProductNameDetailResolver>())
            .ForMember(dest => dest.YieldRate, opt => opt.MapFrom<YieldRateDetailResolver>())
            .ForMember(dest => dest.IsCompletedLate, opt => opt.MapFrom<IsCompletedLateDetailResolver>())
            .ForMember(dest => dest.DaysLate, opt => opt.MapFrom<DaysLateResolver>())
            .ForMember(dest => dest.ScrapReasonName, opt => opt.MapFrom(src => src.ScrapReason != null ? src.ScrapReason.Name : null));
    }
}

/// <summary>
/// Shared computations used by both the list and detail resolvers, kept in one place so the two
/// mappings can never silently drift from each other.
/// </summary>
internal static class WorkOrderCalculations
{
    /// <summary>
    /// Resolves the product name from the work order's related product entity.
    /// </summary>
    internal static string ComputeProductName(WorkOrder src) => src.Product?.Name ?? string.Empty;

    /// <summary>
    /// Resolves the percentage of ordered units successfully stocked, rounded to 2 decimal places.
    /// </summary>
    internal static decimal ComputeYieldRate(WorkOrder src) =>
        src.OrderQty == 0 ? 0m : Math.Round((decimal)src.StockedQty / src.OrderQty * 100, 2);

    /// <summary>
    /// Resolves whether the work order finished after its due date.
    /// </summary>
    internal static bool ComputeIsCompletedLate(WorkOrder src) =>
        src.EndDate.HasValue && src.EndDate.Value > src.DueDate;

    /// <summary>
    /// Resolves the number of calendar days the work order finished past its due date, or null when not completed late.
    /// </summary>
    internal static int? ComputeDaysLate(WorkOrder src) =>
        ComputeIsCompletedLate(src)
            ? (src.EndDate!.Value.Date - src.DueDate.Date).Days
            : null;
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
        WorkOrderCalculations.ComputeProductName(src);
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
        WorkOrderCalculations.ComputeYieldRate(src);
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
        WorkOrderCalculations.ComputeIsCompletedLate(src);
}

/// <summary>
/// Custom resolver for the manufactured product's display name on the detail model.
/// </summary>
public sealed class ProductNameDetailResolver : IValueResolver<WorkOrder, WorkOrderDetailModel, string>
{
    /// <summary>
    /// Resolves the product name from the work order's related product entity.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order detail model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>The product name, or an empty string if the product relationship is not loaded</returns>
    public string Resolve(WorkOrder src, WorkOrderDetailModel dest, string destMember, ResolutionContext context) =>
        WorkOrderCalculations.ComputeProductName(src);
}

/// <summary>
/// Custom resolver for the yield rate computed field on the detail model.
/// </summary>
public sealed class YieldRateDetailResolver : IValueResolver<WorkOrder, WorkOrderDetailModel, decimal>
{
    /// <summary>
    /// Resolves the percentage of ordered units successfully stocked.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order detail model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>StockedQty / OrderQty * 100, rounded to 2 decimal places, or 0 when OrderQty is 0</returns>
    public decimal Resolve(WorkOrder src, WorkOrderDetailModel dest, decimal destMember, ResolutionContext context) =>
        WorkOrderCalculations.ComputeYieldRate(src);
}

/// <summary>
/// Custom resolver for the completed-late computed field on the detail model.
/// </summary>
public sealed class IsCompletedLateDetailResolver : IValueResolver<WorkOrder, WorkOrderDetailModel, bool>
{
    /// <summary>
    /// Resolves whether the work order finished after its due date.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order detail model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>True when <c>EndDate</c> is set and later than <c>DueDate</c>; otherwise false</returns>
    public bool Resolve(WorkOrder src, WorkOrderDetailModel dest, bool destMember, ResolutionContext context) =>
        WorkOrderCalculations.ComputeIsCompletedLate(src);
}

/// <summary>
/// Custom resolver for the days-late computed field.
/// </summary>
public sealed class DaysLateResolver : IValueResolver<WorkOrder, WorkOrderDetailModel, int?>
{
    /// <summary>
    /// Resolves the number of calendar days the work order finished past its due date.
    /// </summary>
    /// <param name="src">the source work order entity</param>
    /// <param name="dest">the destination work order detail model</param>
    /// <param name="destMember">the destination member name</param>
    /// <param name="context">the resolution context</param>
    /// <returns>The whole number of days between the date-truncated EndDate and DueDate, or null when not completed late</returns>
    public int? Resolve(WorkOrder src, WorkOrderDetailModel dest, int? destMember, ResolutionContext context) =>
        WorkOrderCalculations.ComputeDaysLate(src);
}
