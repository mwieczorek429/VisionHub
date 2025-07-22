namespace VisionHub.Api.Models.Cameras
{
    public class CameraDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public List<CameraEventDto> CameraEvents { get; set; } = new();
    }
}
