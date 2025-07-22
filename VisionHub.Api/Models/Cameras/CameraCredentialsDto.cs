namespace VisionHub.Api.Models.Cameras
{
    public class CameraCredentialsDto : CameraCredentialsPayloadDto
    {
        public string CameraName {  get; set; }
        public string CameraUrl { get; set; }
    }
}
