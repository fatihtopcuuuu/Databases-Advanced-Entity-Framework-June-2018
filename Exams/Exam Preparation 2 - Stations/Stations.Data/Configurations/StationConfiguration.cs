using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stations.Models;

namespace Stations.Data.Configurations
{
    public class StationConfiguration : IEntityTypeConfiguration<Station>
    {
        public void Configure(EntityTypeBuilder<Station> builder)
        {
            builder
                .HasAlternateKey(s => s.Name);

            builder
                .HasMany(t => t.TripsFrom)
                .WithOne(s => s.OriginStation)
                .HasForeignKey(s => s.OriginStationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(t => t.TripsTo)
                .WithOne(ds => ds.DestinationStation)
                .HasForeignKey(ds => ds.DestinationStationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
