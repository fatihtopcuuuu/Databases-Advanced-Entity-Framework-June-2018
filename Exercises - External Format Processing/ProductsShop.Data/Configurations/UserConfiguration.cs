using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductShop.Models;

namespace ProductShop.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .HasKey(u => u.Id);

            builder
                .Property(u => u.FirstName)
                .IsRequired(false);

            builder
                .Property(u => u.LastName)
                .IsRequired();

            builder
                .Property(u => u.Age)
                .IsRequired(false);

            builder
                .HasMany(u => u.BoughtProducts)
                .WithOne(p => p.Buyer)
                .HasForeignKey(p => p.BuyerId);

            builder
                .HasMany(u => u.SoldProducts)
                .WithOne(s => s.Seller)
                .HasForeignKey(s => s.SellerId);

            builder
                .ToTable("Users");
        }
    }
}