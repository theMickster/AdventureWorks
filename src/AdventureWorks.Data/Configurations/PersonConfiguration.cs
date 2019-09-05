using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Person", "Person");

            builder.HasKey(a => a.BusinessEntityId);

            builder.HasOne(a => a.BusinessEntity)
                .WithMany(b => b.Persons)
                .HasForeignKey(a => a.BusinessEntityId);
        }
    }
}