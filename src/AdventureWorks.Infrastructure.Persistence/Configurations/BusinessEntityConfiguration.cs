using AdventureWorks.Domain.Entities;
using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class BusinessEntityConfiguration : IEntityTypeConfiguration<BusinessEntity>
{
    public void Configure(EntityTypeBuilder<BusinessEntity> builder)
    {
        builder.ToTable("BusinessEntity", "Person", tableBuilder =>
        {
            // Disable OUTPUT clause - BusinessEntity table may have triggers
            tableBuilder.UseSqlOutputClause(false);
            tableBuilder.HasComment("Table may have database triggers - EF Core OUTPUT clause disabled");
        });

        builder.HasKey(a => a.BusinessEntityId);
    }
}