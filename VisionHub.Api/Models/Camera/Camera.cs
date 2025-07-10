namespace VisionHub.Api.Models.Camera
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
    }
}
