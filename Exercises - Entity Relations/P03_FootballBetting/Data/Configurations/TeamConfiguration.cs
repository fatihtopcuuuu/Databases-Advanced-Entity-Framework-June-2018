namespace P03_FootballBetting.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(t => t.TeamId);

            builder
                .HasOne(t => t.Town);

            builder
                .HasMany(h => h.HomeGames)
                .WithOne(t => t.HomeTeam)
                .HasForeignKey(t => t.HomeTeamId);

            builder
                .HasMany(a => a.AwayGames)
                .WithOne(t => t.AwayTeam)
                .HasForeignKey(t => t.AwayTeamId);

            builder
                .HasMany(p => p.Players)
                .WithOne(t => t.Team)
                .HasForeignKey(t => t.TeamId);
        }
    }
}
