using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VisionHub.Api.Models.Camera;

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
        }
    }
}
