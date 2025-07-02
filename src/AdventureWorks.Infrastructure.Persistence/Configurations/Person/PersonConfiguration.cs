using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Person;

public class PersonConfiguration : IEntityTypeConfiguration<PersonEntity>
{
    public void Configure(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.ToTable("Person", "Person", tableBuilder =>
        {
            // Disable OUTPUT clause - Person.Person table has triggers
            tableBuilder.UseSqlOutputClause(false);
            tableBuilder.HasComment("Table has database triggers - EF Core OUTPUT clause disabled");
        });

        builder.HasKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b => b.Persons)
            .HasForeignKey(a => a.BusinessEntityId);
    }
}