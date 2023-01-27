﻿using AdventureWorks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Persistence.Configurations;

public class BusinessEntityAddressConfiguration : IEntityTypeConfiguration<BusinessEntityAddress>
{
    public void Configure(EntityTypeBuilder<BusinessEntityAddress> builder)
    {
        builder.ToTable("BusinessEntityAddress", "Person");

        builder.HasKey(a => new {a.BusinessEntityId, a.AddressId, a.AddressTypeId });

        builder.HasOne(a => a.BusinessEntity)
            .WithMany(b => b.BusinessEntityAddresses)
            .HasForeignKey(a => a.BusinessEntityId);

        builder.HasOne(a => a.AddressEntity)
            .WithMany()
            .HasForeignKey(a => a.AddressId);

        builder.HasOne(a => a.AddressTypeEntity)
            .WithMany()
            .HasForeignKey(a => a.AddressTypeId);

    }
}