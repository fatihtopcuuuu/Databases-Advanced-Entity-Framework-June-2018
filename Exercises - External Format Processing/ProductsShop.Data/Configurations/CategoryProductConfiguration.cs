using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductShop.Models;

namespace ProductShop.Data.Configurations
{
    public class CategoryProductConfiguration : IEntityTypeConfiguration<CategoryProduct>
    {
        public void Configure(EntityTypeBuilder<CategoryProduct> builder)
        {
            builder
                .HasKey(c => new { c.CategoryId, c.ProductId });

            builder
                .HasOne(p => p.Product)
                .WithMany(c => c.CategoryProducts)
                .HasForeignKey(p => p.ProductId);

            builder
                .HasOne(c => c.Category)
                .WithMany(cp => cp.CategoryProducts)
                .HasForeignKey(c => c.CategoryId);

            builder
                .ToTable("CategoryProducts");
        }
    }
}