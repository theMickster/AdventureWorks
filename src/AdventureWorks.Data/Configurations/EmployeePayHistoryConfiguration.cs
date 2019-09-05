using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class EmployeePayHistoryConfiguration : IEntityTypeConfiguration<EmployeePayHistory>
    {
        public void Configure(EntityTypeBuilder<EmployeePayHistory> builder)
        {
            builder.ToTable("EmployeePayHistory", "HumanResources");

            builder.HasKey(a => new { a.BusinessEntityId, a.RateChangeDate });

            builder.HasOne(a => a.BusinessEntity)
                .WithMany(b => b.EmployeePayHistory)
                .HasForeignKey(a => a.BusinessEntityId);
        }
    }
}
