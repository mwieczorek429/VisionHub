namespace VisionHub.Api.Models.Cameras
{
    public class CameraAddRequestDto : CameraLoginRequestDto
    {
        public string CameraName { get; set; }
        public string CameraUrl { get; set; }
    }
}
