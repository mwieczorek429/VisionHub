using VisionHub.Api.Models.Auth;

namespace VisionHub.Api.Models.Cameras
{
    public class Camera
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }
        public string Url { get; set; }
        public ICollection<CameraEvent>? CameraEvents { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
    }
}
