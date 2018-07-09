namespace Instagraph.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder
                .HasMany(uf => uf.Comments)
                .WithOne(u => u.Post)
                .HasForeignKey(u => u.PostId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
