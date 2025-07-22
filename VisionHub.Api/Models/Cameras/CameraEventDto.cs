namespace VisionHub.Api.Models.Cameras
{
    public class CameraEventDto
    {
        public int Id { get; set; }
        public bool MotionDetected { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Object { get; set; }
    }
}
