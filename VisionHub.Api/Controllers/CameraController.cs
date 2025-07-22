using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisionHub.Api.Constants;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ICameraRepository _cameraRepository;
        private readonly ICameraEventRepository _cameraEventRepository;
        private readonly ILogger<CameraController> _logger;

        public CameraController(HttpClient httpClient, ICameraRepository cameraRepository, ICameraEventRepository cameraEventRepository, ILogger<CameraController> logger)
        {
            _httpClient = httpClient;
            _cameraRepository = cameraRepository;
            _cameraEventRepository = cameraEventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a list of all cameras in the system.
        /// </summary>
        /// <returns>List of camera summaries (CameraSummaryDto).</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CameraSummaryDto>>> GetAllCameras()
        {
            var cameras = await _cameraRepository.GetAllSummariesAsync();
            return Ok(cameras);
        }

        /// <summary>
        /// Retrieves details of a camera by its ID.
        /// </summary>
        /// <param name="id">Camera identifier.</param>
        /// <returns>Camera details (CameraDetailsDto) or NotFound if the camera does not exist.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<CameraDetailsDto>> GetCameraDetails(int id)
        {
            var camera = await _cameraRepository.GetCameraDetailsAsync(id);

            if (camera == null)
                return NotFound($"Camera with ID {id} not found.");

            return Ok(camera);
        }

        /// <summary>
        /// Adds a new camera to the system.
        /// </summary>
        /// <param name="request">Data for adding the camera, including URL, login, and password.</param>
        /// <returns>Camera token or error in case of login failure or server error.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> AddCamera([FromBody] CameraAddRequestDto request)
        {
            _logger.LogInformation("AddCamera called with URL: {CameraUrl}", request.CameraUrl);

            try
            {
                var loginResponse = await _httpClient.PostAsJsonAsync(request.CameraUrl + MdcsApiEndpoints.Login, new CameraLoginRequestDto
                {
                    Login = request.Login,
                    Password = request.Password,
                });

                if (!loginResponse.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to login to camera at {CameraUrl} with status code {StatusCode}", request.CameraUrl, loginResponse.StatusCode);
                    return StatusCode((int)loginResponse.StatusCode, "Failed to login to the camera.");
                }

                var result = await loginResponse.Content.ReadFromJsonAsync<CameraTokenResponseDto>();
                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    _logger.LogError("Token response was null or empty for camera at {CameraUrl}", request.CameraUrl);
                    return StatusCode(500, "Unable to read token from the camera response.");
                }

                _cameraRepository.AddCamera(new Camera
                {
                    Login = request.Login,
                    Password = request.Password,
                    Name = request.CameraName,
                    Token = result.Token,
                    Url = request.CameraUrl,
                });

                _logger.LogInformation("Camera added successfully: {CameraName}", request.CameraName);

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while adding camera at {CameraUrl}", request.CameraUrl);
                return StatusCode(503, $"Error connecting to camera: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding camera");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }



        /// <summary>
        /// Updates the authentication credentials of a camera.
        /// </summary>
        /// <param name="cameraId">ID of the camera to update.</param>
        /// <param name="credentialsDto">New authentication credentials.</param>
        /// <returns>Operation status.</returns>
        [HttpPatch("{cameraId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> EditCameraCredentials(int cameraId, [FromBody] CameraCredentialsDto credentialsDto)
        {
            var camera = _cameraRepository.GetCameraById(cameraId);
            if (camera == null)
            {
                return NotFound($"Camera with ID {cameraId} not found.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", camera.Token);

            var payload = new CameraCredentialsPayloadDto
            {
                CurrentLogin = credentialsDto.CurrentLogin,
                CurrentPassword = credentialsDto.CurrentPassword,
                NewLogin = credentialsDto.NewLogin,
                NewPassword = credentialsDto.NewPassword
            };

            try
            {
                var response = await _httpClient.PatchAsJsonAsync(camera.Url + MdcsApiEndpoints.Credentials, payload);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Failed to update camera credentials.");
                }

                if (!string.IsNullOrEmpty(credentialsDto.NewLogin))
                    camera.Login = credentialsDto.NewLogin;

                if (!string.IsNullOrEmpty(credentialsDto.NewPassword))
                    camera.Password = credentialsDto.NewPassword;

                if (!string.IsNullOrEmpty(credentialsDto.CameraName))
                    camera.Name = credentialsDto.CameraName;

                if (!string.IsNullOrEmpty(credentialsDto.CameraUrl))
                    camera.Url = credentialsDto.CameraUrl;

                _cameraRepository.UpdateCamera(camera);

                return Ok("Camera credentials updated successfully.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"HTTP error when updating credentials: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a camera by its ID.
        /// </summary>
        /// <param name="cameraId">ID of the camera to delete.</param>
        /// <returns>Deletion status.</returns>
        [HttpDelete("{cameraId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCamera(int cameraId)
        {
            var camera = _cameraRepository.GetCameraById(cameraId);
            if (camera == null)
            {
                return NotFound($"Camera with ID {cameraId} not found.");
            }

            _cameraRepository.DeleteCamera(cameraId);
            return Ok($"Camera with ID {cameraId} has been deleted.");
        }


        /// <summary>
        /// Retrieves all camera events.
        /// </summary>
        /// <returns>List of camera events.</returns>
        [HttpGet("events")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CameraEventWithCameraIdDto>>> GetAllCameraEvents()
        {
            var events = await _cameraEventRepository.GetCameraEventsAsync();
            return Ok(events);
        }

        /// <summary>
        /// Retrieves the latest camera event.
        /// </summary>
        /// <returns>Latest camera event or NotFound if none exists.</returns>
        [HttpGet("events/latest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CameraEventWithCameraIdDto>> GetLastCameraEvent()
        {
            var lastEvent = await _cameraEventRepository.GetLastCameraEventAsync();
            if (lastEvent == null)
            {
                return NotFound("No camera events found.");
            }

            return Ok(lastEvent);
        }
    }
}
