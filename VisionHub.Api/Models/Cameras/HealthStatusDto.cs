namespace VisionHub.Api.Models.Cameras
{
    public class HealthStatusDto
    {
        public bool DatabaseConnected { get; set; }
        public List<CameraHealthStatusDto> CameraConnections { get; set; }
    }
}
