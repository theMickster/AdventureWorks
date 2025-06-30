using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeEntity>
{
    public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
    {
        builder.ToTable("Employee", "HumanResources");

        builder.HasKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.PersonBusinessEntity)
            .WithMany(b=>b.Employees)
            .HasForeignKey(a => a.BusinessEntityId);

            
    }
}