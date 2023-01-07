namespace AdventureWorks.Domain.Entities;

public class CurrencyRate : BaseEntity
{
    public int CurrencyRateId { get; set; }
    public DateTime CurrencyRateDate { get; set; }
    public string FromCurrencyCode { get; set; }
    public string ToCurrencyCode { get; set; }
    public decimal AverageRate { get; set; }
    public decimal EndOfDayRate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    public Currency FromCurrencyCodeNavigation { get; set; }
    public Currency ToCurrencyCodeNavigation { get; set; }
}