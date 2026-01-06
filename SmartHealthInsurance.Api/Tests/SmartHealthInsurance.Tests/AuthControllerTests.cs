using Moq;
using Xunit;
using SmartHealthInsurance.Api.Controllers;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;

namespace SmartHealthInsurance.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsValid()
        {
            // Arrange
            var dto = new LoginDto { Email = "test@user.com", Password = "Password123" };
            var response = new AuthResponseDto { Token = "jwt_token", Role = "Customer" };

            _mockAuthService.Setup(s => s.LoginAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value as AuthResponseDto;
            Assert.Equal("jwt_token", value!.Token);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenServiceThrows()
        {
            // Arrange
            var dto = new LoginDto { Email = "test@user.com", Password = "WrongPassword" };
            
            _mockAuthService.Setup(s => s.LoginAsync(dto))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Invalid credentials", unauthorizedResult.Value!.ToString());
        }
    }
}
