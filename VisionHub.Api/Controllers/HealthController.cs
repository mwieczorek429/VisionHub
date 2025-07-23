using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var cameraStatuses = new List<CameraHealthStatusDto>();

            foreach (var camera in cameras)
            {
                var statusUrl = camera.Url.TrimEnd('/') + "/camera/status";
                var request = new HttpRequestMessage(HttpMethod.Get, statusUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", camera.Token);

                bool isConnected;
                try
                {
                    var response = await httpClient.SendAsync(request);
                    isConnected = response.IsSuccessStatusCode;
                }
                catch
                {
                    isConnected = false;
                }

                cameraStatuses.Add(new CameraHealthStatusDto
                {
                    Id = camera.Id,
                    Name = camera.Name,
                    Connected = isConnected
                });
            }

            return Ok(new HealthStatusDto
            {
                DatabaseConnected = dbConnected,
                CameraConnections = cameraStatuses
            });
        }
    }
}
