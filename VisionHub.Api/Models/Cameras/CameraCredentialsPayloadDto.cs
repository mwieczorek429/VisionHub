namespace VisionHub.Api.Models.Cameras
{
    public class CameraCredentialsPayloadDto
    {
        public string CurrentLogin { get; set; }
        public string CurrentPassword { get; set; }
        public string NewLogin { get; set; }
        public string NewPassword { get; set; }
    }
}
