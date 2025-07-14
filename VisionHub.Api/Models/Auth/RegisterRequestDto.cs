namespace VisionHub.Api.Models.Auth
{
    public class RegisterRequestDto
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
