using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Controllers
{
    /// <summary>
    /// Controller for checking the health status of the application and connected cameras.
    /// </summary>
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

        /// <summary>
        /// Checks the health status of the database connection and all registered cameras.
        /// </summary>
        /// <remarks>
        /// This endpoint verifies the connection to the database and attempts to reach each camera's status endpoint using their respective tokens.
        /// </remarks>
        /// <returns>
        /// A status object containing:
        /// - `DatabaseConnected`: Indicates whether the database connection is active.
        /// - `CameraConnections`: A list of cameras with their individual connectivity status.
        /// </returns>
        /// <response code="200">Returns the health status of the system and cameras.</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
