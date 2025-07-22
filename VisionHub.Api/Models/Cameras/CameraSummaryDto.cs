namespace VisionHub.Api.Models.Cameras
{
    public class CameraSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public DateTimeOffset? LastEventTimestamp { get; set; }
        public string? LastEventDetectedObject { get; set; }
        public bool? LastEventMotionDetected { get; set; }
    }
}
