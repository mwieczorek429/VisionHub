using Azure.Core;
using VisionHub.Api.Constants;
using VisionHub.Api.Models.Camera;

namespace VisionHub.Api.Services
{
    public class CameraService
    {
        private readonly ICameraRepository _cameraRepository;
        private readonly HttpClient _httpClient;
        public CameraService(ICameraRepository cameraRepository, HttpClient httpClient) 
        {
            _httpClient = httpClient;
            _cameraRepository = cameraRepository;
        }

        private async Task<string> GetNewTokenAsync(Camera camera)
        {
            var response = await _httpClient.PostAsJsonAsync(camera.Url + MdcsApiEndpoints.Login, new CameraLoginRequestDto
            {
                Login = camera.Login,
                Password = camera.Password,
            });

            var result = await response.Content.ReadFromJsonAsync<CameraTokenResponseDto>();
            return result.Token;
        }

        public async Task UpdateTokenAsync(Camera camera)
        {
            camera.Token = await GetNewTokenAsync(camera);
            await _cameraRepository.UpdateCameraAsync(camera);
        }
    }
}
