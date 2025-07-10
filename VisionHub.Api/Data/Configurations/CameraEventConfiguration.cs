using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VisionHub.Api.Models.Camera;

namespace VisionHub.Api.Data.Configurations
{
    public class CameraEventConfiguration : IEntityTypeConfiguration<CameraEvent>
    {
        public void Configure(EntityTypeBuilder<CameraEvent> builder)
        {
            builder.HasKey(ce => ce.Id);

            builder.HasOne(ce => ce.Camera)
                .WithMany(c => c.CameraEvents)
                .HasForeignKey(ce => ce.CameraId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
