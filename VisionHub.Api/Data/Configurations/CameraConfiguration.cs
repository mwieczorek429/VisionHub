using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Data.Configurations
{
    public class CameraConfiguration : IEntityTypeConfiguration<Camera>
    {
        public void Configure(EntityTypeBuilder<Camera> builder)
        {
            builder.HasKey(c => c.Id);

            builder.HasMany(c => c.CameraEvents)
                .WithOne(ce => ce.Camera)
                .HasForeignKey(ce => ce.CameraId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(c => c.AppUser)
                   .WithMany(u => u.Cameras)
                   .HasForeignKey(c => c.AppUserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
