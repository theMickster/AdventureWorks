using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employee", "HumanResources");

            builder.HasKey(a => a.BusinessEntityId);

            builder.HasOne(a => a.PersonBusinessEntity)
                .WithMany(b=>b.Employees)
                .HasForeignKey(a => a.BusinessEntityId);

            
        }
    }
}
