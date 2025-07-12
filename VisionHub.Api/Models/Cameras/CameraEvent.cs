namespace VisionHub.Api.Models.Cameras
{
    public class CameraEvent
    {
        public int Id { get; set; }
        public bool MotionDetected { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Object { get; set; }
        public int CameraId { get; set; }
        public Camera Camera { get; set; }
    }
}
