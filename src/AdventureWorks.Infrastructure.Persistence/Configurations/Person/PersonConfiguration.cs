using AdventureWorks.Domain.Entities.Person;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations.Person;

public class PersonConfiguration : IEntityTypeConfiguration<PersonEntity>
{
    public void Configure(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.ToTable("Person", "Person");

        builder.HasKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b => b.Persons)
            .HasForeignKey(a => a.BusinessEntityId);
    }
}