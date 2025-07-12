using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using VisionHub.Api.Constants;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ICameraRepository _cameraRepository;
        private readonly ICameraEventRepository _cameraEventRepository;

        public CameraController(HttpClient httpClient, ICameraRepository cameraRepository, ICameraEventRepository cameraEventRepository) 
        {
            _httpClient = httpClient;
            _cameraRepository = cameraRepository;
            _cameraEventRepository = cameraEventRepository;
        }

        [HttpGet]
        public List<Camera> GetAllCameras()
        {
            return _cameraRepository.Cameras.ToList();
        }

        [HttpGet("{cameraId}")]
        public Camera GetCameraById(int cameraId)
        {
            return _cameraRepository.GetCameraById(cameraId);
        }

        [HttpPost]
        public async Task<CameraTokenResponseDto?> AddCamera([FromBody] CameraAddRequestDto request)
        {
            var response = await _httpClient.PostAsJsonAsync(request.CameraUrl + MdcsApiEndpoints.Login, new CameraLoginRequestDto
            {
                Login = request.Login,
                Password = request.Password,
            });
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CameraTokenResponseDto>();
                _cameraRepository.AddCamera(new Camera
                {
                    Login = request.Login,
                    Password = request.Password,
                    Name = "Kamera",
                    Token = result.Token,
                    Url = request.CameraUrl
                });

                return result;
            }

            return null;
        }

        [HttpPatch("{cameraId}")]
        public async Task<IActionResult> EditCameraCredentials(int cameraId, [FromBody]CameraCredentialsDto credentialsDto)
        {
            Camera camera = _cameraRepository.GetCameraById(cameraId);
            if (camera != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", camera.Token);
                var response = await _httpClient.PatchAsJsonAsync(camera.Url + MdcsApiEndpoints.Credentials, credentialsDto);

                if (response.IsSuccessStatusCode) 
                {

                    if (!string.IsNullOrEmpty(credentialsDto.NewLogin)) 
                    {
                        camera.Login = credentialsDto.NewLogin;
                    }

                    if (!string.IsNullOrEmpty(credentialsDto.NewPassword)) 
                    {
                        camera.Password = credentialsDto.NewPassword;
                    }

                    _cameraRepository.UpdateCamera(camera);

                    return Ok("Data changed");
                }
                return StatusCode((int)response.StatusCode, "Failed to update camera credentials.");
            }

            return NotFound();
        }

        [HttpDelete("{cameraId}")]
        public IActionResult DeleteCamera(int cameraId) 
        {
            _cameraRepository.DeleteCamera(cameraId);

            return Ok();
        }

        [HttpGet("events")]
        public List<CameraEvent> GetAllCameraEvents()
        {
            return _cameraEventRepository.CameraEvents.ToList();
        }

        [HttpGet("events/{cameraId}")]
        public async Task<IEnumerable<CameraEvent>> GetCameraEvents(int cameraId)
        {
            return await _cameraEventRepository.GetCameraEventsAsync(cameraId);
        }

        [HttpGet("events/latest/{cameraId}")]
        public Task<CameraEvent> GetLastCameraEvent(int cameraId)
        {
            return _cameraEventRepository.GetLastCameraEventAsync(cameraId);
        }
    }
}
