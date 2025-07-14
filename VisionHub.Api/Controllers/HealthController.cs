using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ICameraRepository _cameraRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public HealthController(ICameraRepository cameraRepository, IHttpClientFactory httpClientFactory)
        {
            _cameraRepository = cameraRepository;
            _httpClientFactory = httpClientFactory;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetHealthStatus()
        {
            var dbConnected = _cameraRepository.CanConnect();

            var cameras = _cameraRepository.Cameras.ToList();
            var httpClient = _httpClientFactory.CreateClient();

            var cameraStatuses = new List<object>();

            foreach (var camera in cameras)
            {
                var statusUrl = camera.Url.TrimEnd('/') + "/camera/status";
                var request = new HttpRequestMessage(HttpMethod.Get, statusUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", camera.Token);

                try
                {
                    var response = await httpClient.SendAsync(request);
                    cameraStatuses.Add(new { camera.Id, camera.Name, Connected = response.IsSuccessStatusCode });
                }
                catch
                {
                    cameraStatuses.Add(new { camera.Id, camera.Name, Connected = false });
                }
            }

            return Ok(new
            {
                DatabaseConnected = dbConnected,
                CameraConnections = cameraStatuses
            });
        }
    }
}
