namespace Stations.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    public class StationConfiguration : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {
            builder
                .HasAlternateKey(s => s.Name);

            builder
                .HasMany(t => t.TripsTo)
                .WithOne(d => d.DestinationStation)
                .HasForeignKey(d => d.DestinationStationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(tf => tf.TripsFrom)
                .WithOne(o => o.OriginStation)
                .HasForeignKey(o => o.OriginStationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
