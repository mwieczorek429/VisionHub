using System.Text.Json.Serialization;

namespace VisionHub.Api.Models.Camera
{
    public class CameraLoginRequestDto
    {
        [JsonPropertyName("username")]
        public string Login {  get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
