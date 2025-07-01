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

        // OrganizationLevel is a computed column in the database
        // Mark it as computed so EF Core won't try to insert/update it
        builder.Property(e => e.OrganizationLevel)
            .ValueGeneratedOnAddOrUpdate();
    }
}