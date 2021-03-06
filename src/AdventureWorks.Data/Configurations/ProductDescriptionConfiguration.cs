﻿using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ProductDescriptionConfiguration : IEntityTypeConfiguration<ProductDescription>
    {
        public void Configure(EntityTypeBuilder<ProductDescription> builder)
        {
            builder.ToTable("ProductDescription", "Production");

            builder.HasKey(a => a.ProductDescriptionId);
        }
    }
}