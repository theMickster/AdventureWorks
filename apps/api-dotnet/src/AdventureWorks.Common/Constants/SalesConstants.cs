namespace AdventureWorks.Common.Constants;

/// <summary>
/// Constants for Sales domain business rules and sentinels.
/// </summary>
public static class SalesConstants
{
    /// <summary>
    /// Sentinel thrown by <c>StoreRepository.ReassignSalesPersonAsync</c> when the new SalesPersonId
    /// matches the store's current SalesPersonId inside the transaction (TOCTOU guard).
    /// The handler catches this and converts it to a Rule-02 ValidationException.
    /// </summary>
    public const string SameSalesPersonSentinel = "ERR_SAME_SALES_PERSON";
}
