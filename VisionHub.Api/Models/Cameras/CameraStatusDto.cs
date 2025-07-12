using System.Text.Json.Serialization;

namespace VisionHub.Api.Models.Cameras
{
    public class CameraStatusDto
    {
        [JsonPropertyName("motionDetected")]
        public bool MotionDetected { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
        [JsonPropertyName("object")]
        public string Object {  get; set; }
    }
}
