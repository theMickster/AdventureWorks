using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Address", "Person");

            builder.HasKey(a => a.AddressId);

            builder.HasOne(a => a.StateProvince)
                .WithMany()
                .HasForeignKey(a => a.StateProvinceId);
        }
    }
}
