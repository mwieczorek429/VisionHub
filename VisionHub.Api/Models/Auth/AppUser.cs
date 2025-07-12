using System.ComponentModel.DataAnnotations;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Models.Auth
{
    public class AppUser
    {
        public int Id { get; set; }
        [Required]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public ICollection<Camera> Cameras { get; set; } = new List<Camera>();


    }
}
