using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class BillOfMaterialsConfiguration : IEntityTypeConfiguration<BillOfMaterials>
    {
        public void Configure(EntityTypeBuilder<BillOfMaterials> builder)
        {
            builder.ToTable("BillOfMaterials", "Production");

            builder.HasKey(a => a.BillOfMaterialsId);

            builder.HasOne(a => a.Component)
                .WithMany(b=>b.BillOfMaterialsComponents)
                .HasForeignKey(a => a.ComponentId);

            builder.HasOne(a => a.ProductAssembly)
                .WithMany(b=>b.BillOfMaterialsProductAssemblies)
                .HasForeignKey(a => a.ProductAssemblyId);

            builder.HasOne(a => a.UnitMeasureCodeNavigation)
                .WithMany(b=>b.BillOfMaterials)
                .HasForeignKey(a => a.UnitMeasureCode);
        }
    }
}