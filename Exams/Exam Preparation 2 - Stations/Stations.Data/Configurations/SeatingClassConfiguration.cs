namespace Stations.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    public class SeatingClassConfiguration : IEntityTypeConfiguration<SeatingClass>
    {
        public void Configure(EntityTypeBuilder<SeatingClass> builder)
        {
            builder
                .HasAlternateKey(sc => sc.Abbreviation);

            builder
                .HasAlternateKey(sc => sc.Name);
        }
    }
}
