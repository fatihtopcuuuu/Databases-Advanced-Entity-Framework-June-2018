namespace P03_FootballBetting.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    public class ColorConfiguration : IEntityTypeConfiguration<Color>
    {
        public void Configure(EntityTypeBuilder<Color> builder)
        {
            builder.HasKey(c => c.ColorId);

            builder
                .HasMany(pt => pt.PrimaryKitTeams)
                .WithOne(pc => pc.PrimaryKitColor)
                .HasForeignKey(pc => pc.PrimaryKitColorId);

            builder
                .HasMany(st => st.SecondaryKitTeams)
                .WithOne(sc => sc.SecondaryKitColor)
                .HasForeignKey(sc => sc.SecondaryKitColorId);
        }
    }
}
