using Microsoft.Extensions.Logging;
using Moq;
using System.Net.Http.Json;
using System.Net;
using VisionHub.Api.Controllers;
using VisionHub.Api.Models.Cameras;
using Microsoft.AspNetCore.Mvc;

namespace VisionHub.Api.Tests.Controllers
{
    public class CameraControllerTests
    {
        private readonly Mock<ICameraRepository> _cameraRepoMock = new();
        private readonly Mock<ICameraEventRepository> _eventRepoMock = new();
        private readonly Mock<ILogger<CameraController>> _loggerMock = new();

        private CameraController CreateController(HttpClient httpClient = null)
        {
            return new CameraController(
                httpClient ?? new HttpClient(new MockHttpMessageHandler()),
                _cameraRepoMock.Object,
                _eventRepoMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetAllCameras_ReturnsOkWithData()
        {
            _cameraRepoMock.Setup(r => r.GetAllSummariesAsync())
                .ReturnsAsync(new List<CameraSummaryDto> { new CameraSummaryDto() });

            var controller = CreateController();

            var result = await controller.GetAllCameras();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsAssignableFrom<List<CameraSummaryDto>>(ok.Value);
        }

        [Fact]
        public async Task GetCameraDetails_CameraFound_ReturnsOk()
        {
            _cameraRepoMock.Setup(r => r.GetCameraDetailsAsync(1))
                .ReturnsAsync(new CameraDetailsDto());

            var controller = CreateController();
            var result = await controller.GetCameraDetails(1);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetCameraDetails_CameraNotFound_ReturnsNotFound()
        {
            _cameraRepoMock.Setup(r => r.GetCameraDetailsAsync(99)).ReturnsAsync((CameraDetailsDto)null);
            var controller = CreateController();
            var result = await controller.GetCameraDetails(99);
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLastCameraEvent_Exists_ReturnsOk()
        {
            _eventRepoMock.Setup(r => r.GetLastCameraEventAsync())
                .ReturnsAsync(new CameraEventWithCameraIdDto());

            var controller = CreateController();
            var result = await controller.GetLastCameraEvent();
            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetLastCameraEvent_NotFound_ReturnsNotFound()
        {
            _eventRepoMock.Setup(r => r.GetLastCameraEventAsync())
                .ReturnsAsync((CameraEventWithCameraIdDto)null);

            var controller = CreateController();
            var result = await controller.GetLastCameraEvent();
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public void DeleteCamera_Found_ReturnsOk()
        {
            _cameraRepoMock.Setup(r => r.GetCameraById(1)).Returns(new Camera());

            var controller = CreateController();
            var result = controller.DeleteCamera(1);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void DeleteCamera_NotFound_ReturnsNotFound()
        {
            _cameraRepoMock.Setup(r => r.GetCameraById(1)).Returns((Camera)null);
            var controller = CreateController();

            var result = controller.DeleteCamera(1);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task AddCamera_ValidResponse_ReturnsOk()
        {
            var controller = CreateController();
            var request = new CameraAddRequestDto
            {
                CameraUrl = "http://fake-camera",
                Login = "admin",
                Password = "pass",
                CameraName = "Cam1"
            };

            var result = await controller.AddCamera(request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var responseDto = Assert.IsType<CameraTokenResponseDto>(okResult.Value);
            Assert.Equal("mocked-token", responseDto.Token);
        }

        [Fact]
        public async Task EditCameraCredentials_CameraFound_ReturnsOk()
        {
            _cameraRepoMock.Setup(r => r.GetCameraById(1)).Returns(new Camera
            {
                Id = 1,
                Token = "mocked-token",
                Url = "http://fake-camera"
            });

            var controller = CreateController();

            var result = await controller.EditCameraCredentials(1, new CameraCredentialsDto
            {
                CurrentLogin = "admin",
                CurrentPassword = "pass",
                NewLogin = "newadmin",
                NewPassword = "newpass"
            });

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Camera credentials updated successfully.", okResult.Value);
        }

        [Fact]
        public async Task EditCameraCredentials_CameraNotFound_ReturnsNotFound()
        {
            _cameraRepoMock.Setup(r => r.GetCameraById(1)).Returns((Camera)null);
            var controller = CreateController();

            var result = await controller.EditCameraCredentials(1, new CameraCredentialsDto());

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new CameraTokenResponseDto { Token = "mocked-token" })
            };

            return Task.FromResult(response);
        }
    }
}
