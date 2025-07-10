using Microsoft.EntityFrameworkCore;
using VisionHub.Api.Models.Camera;

namespace VisionHub.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraEvent> CameraEvents { get; set; }
    }
}
