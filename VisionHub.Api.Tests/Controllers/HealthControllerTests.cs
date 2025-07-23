using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;
using VisionHub.Api.Controllers;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Tests.Controllers
{
    public class HealthControllerTests
    {
        [Fact]
        public async Task GetHealthStatus_ReturnsCameraStatusesAndDbConnection()
        {
            // Arrange
            var cameraRepoMock = new Mock<ICameraRepository>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            cameraRepoMock.Setup(r => r.CanConnect()).Returns(true);
            cameraRepoMock.Setup(r => r.Cameras).Returns(new List<Camera>
        {
            new Camera { Id = 1, Name = "Cam1", Url = "http://camera1", Token = "token1" },
            new Camera { Id = 2, Name = "Cam2", Url = "http://camera2", Token = "token2" }
        }.AsQueryable());

            var handler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(handler);

            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new HealthController(cameraRepoMock.Object, httpClientFactoryMock.Object);

            // Act
            var result = await controller.GetHealthStatus();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var healthStatus = Assert.IsType<HealthStatusDto>(okResult.Value);

            Assert.True(healthStatus.DatabaseConnected);
            Assert.Equal(2, healthStatus.CameraConnections.Count);
            Assert.All(healthStatus.CameraConnections, c => Assert.True(c.Connected));
        }

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }
        }
    }
}