namespace P01_StudentSystem.Data.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Models;

    public class CourseConfiguration : IEntityTypeConfiguration<Course>

    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.CourseId);

            builder
                .Property(c => c.Name)
                .HasMaxLength(80)
                .IsRequired()
                .IsUnicode();

            builder
                .Property(c => c.Description)
                .IsRequired(false)
                .IsUnicode();

            builder
                .Property(c => c.StartDate)
                .HasColumnType("DATETIME2");

            builder
                .Property(c => c.EndDate)
                .HasColumnType("DATETIME2");
        }
    }
}
