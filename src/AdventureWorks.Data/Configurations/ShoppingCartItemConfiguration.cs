using AdventureWorks.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdventureWorks.Infrastructure.Configurations
{
    public class ShoppingCartItemConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
    {
        public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
        {
            builder.ToTable("ShoppingCartItem", "Sales");

            builder.HasKey(a => a.ShoppingCartItemId);

            builder.HasOne(a => a.Product)
                .WithMany(b => b.ShoppingCartItems)
                .HasForeignKey(a => a.ProductId);
        }
    }
}