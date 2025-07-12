using Microsoft.EntityFrameworkCore;
using VisionHub.Api.Models.Auth;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraEvent> CameraEvents { get; set; }
        public DbSet<AppUser> Users { get; set; }
    }
}
