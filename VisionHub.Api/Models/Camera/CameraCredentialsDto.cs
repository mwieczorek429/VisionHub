namespace VisionHub.Api.Models.Camera
{
    public class CameraCredentialsDto
    {
        public string CurrentLogin { get; set; }
        public string CurrentPassword { get; set; }
        public string NewLogin { get; set; }
        public string NewPassword { get; set; }
    }
}
