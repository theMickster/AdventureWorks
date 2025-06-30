using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class EmployeeDepartmentHistoryConfiguration : IEntityTypeConfiguration<EmployeeDepartmentHistory>
{
    public void Configure(EntityTypeBuilder<EmployeeDepartmentHistory> builder)
    {
        builder.ToTable("EmployeeDepartmentHistory", "HumanResources");

        builder.HasKey(a => new {a.BusinessEntityId, a.DepartmentId, a.ShiftId, a.StartDate});

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b => b.EmployeeDepartmentHistory)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.Department)
            .WithMany(b => b.EmployeeDepartmentHistory)
            .HasForeignKey(a => a.DepartmentId);

        builder.HasOne(a => a.Shift)
            .WithMany(b => b.EmployeeDepartmentHistory)
            .HasForeignKey(a => a.ShiftId);
    }
}