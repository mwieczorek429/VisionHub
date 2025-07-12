using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VisionHub.Api.Models.Auth;

namespace VisionHub.Api.Data.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Login)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                   .IsRequired();

            builder.HasIndex(u => u.Login)
                   .IsUnique();

            builder.HasMany(u => u.Cameras)
                   .WithOne(c => c.AppUser)
                   .HasForeignKey(c => c.AppUserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
