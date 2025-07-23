using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;
using VisionHub.Api.Controllers;
using VisionHub.Api.Models.Auth;
using VisionHub.Api.Models.Cameras;

namespace VisionHub.Api.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAppUserRepository> _mockUserRepo = new();
        private readonly Mock<IConfiguration> _mockConfig = new();

        private AuthController CreateController()
        {
            return new AuthController(_mockUserRepo.Object, _mockConfig.Object);
        }

        [Fact]
        public async Task Register_UserDoesNotExist_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            var request = new RegisterRequestDto { Login = "testuser", Password = "pass123" };
            _mockUserRepo.Setup(r => r.UserExistsAsync(request.Login)).ReturnsAsync(false);
            _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User registered successfully.", okResult.Value);
        }

        [Fact]
        public async Task Register_UserExists_ReturnsBadRequest()
        {
            var controller = CreateController();
            var request = new RegisterRequestDto { Login = "existinguser", Password = "pass123" };
            _mockUserRepo.Setup(r => r.UserExistsAsync(request.Login)).ReturnsAsync(true);

            var result = await controller.Register(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User already exists.", badRequest.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange
            var user = new AppUser
            {
                Id = 1,
                Login = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123")
            };

            _mockUserRepo.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(user);
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("supersecretkeysupersecretkey1234");

            var controller = CreateController();

            var request = new LoginRequestDto { Login = user.Login, Password = "pass123" };

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var tokenProp = okResult.Value.GetType().GetProperty("token");
            Assert.NotNull(tokenProp);

            var tokenValue = tokenProp.GetValue(okResult.Value) as string;
            Assert.False(string.IsNullOrEmpty(tokenValue));
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var user = new AppUser
            {
                Id = 1,
                Login = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass123")
            };

            _mockUserRepo.Setup(r => r.GetByLoginAsync(user.Login)).ReturnsAsync(user);

            var controller = CreateController();

            var request = new LoginRequestDto { Login = user.Login, Password = "wrongpass" };

            var result = await controller.Login(request);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid username or password.", unauthorized.Value);
        }

        [Fact]
        public async Task ChangePassword_ValidRequest_ReturnsOk()
        {
            // Arrange
            var user = new AppUser
            {
                Id = 1,
                Login = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass")
            };

            _mockUserRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<AppUser>())).Returns(Task.CompletedTask);

            var controller = CreateController();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaims }
            };

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "oldpass",
                NewPassword = "newpass"
            };

            // Act
            var result = await controller.ChangePassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Password updated successfully.", okResult.Value);
        }

        [Fact]
        public async Task ChangePassword_InvalidCurrentPassword_ReturnsBadRequest()
        {
            var user = new AppUser
            {
                Id = 1,
                Login = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass")
            };

            _mockUserRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var controller = CreateController();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaims }
            };

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "wrongpass",
                NewPassword = "newpass"
            };

            var result = await controller.ChangePassword(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Current password is incorrect.", badRequest.Value);
        }

        [Fact]
        public async Task ChangePassword_UserNotFound_ReturnsNotFound()
        {
            _mockUserRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((AppUser?)null);

            var controller = CreateController();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "999")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = userClaims }
            };

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "any",
                NewPassword = "newpass"
            };

            var result = await controller.ChangePassword(request);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFound.Value);
        }

        [Fact]
        public async Task ChangePassword_Unauthorized_ReturnsUnauthorized()
        {
            var controller = CreateController();

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
            };

            var request = new ChangePasswordRequestDto
            {
                CurrentPassword = "oldpass",
                NewPassword = "newpass"
            };

            var result = await controller.ChangePassword(request);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Invalid user ID.", unauthorized.Value);
        }
    }
}
