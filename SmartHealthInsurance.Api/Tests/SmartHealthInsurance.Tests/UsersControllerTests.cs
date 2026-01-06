using Moq;
using Xunit;
using SmartHealthInsurance.Api.Controllers;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using SmartHealthInsurance.Api.Data; // For ApplicationDbContext if needed
using System.Threading.Tasks;
using System;

namespace SmartHealthInsurance.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IPolicyService> _mockPolicyService;
        // private readonly Mock<ApplicationDbContext> _mockContext; // DbContext mocking is hard, pass null if unused

        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAuthService = new Mock<IAuthService>();
            _mockPolicyService = new Mock<IPolicyService>();
            
            // We pass null for DbContext since CreateUser doesn't use it, 
            // and mocking concrete DbContext requires more setup or an Interface wrapper.
            // If the controller throws NRE in constructor it's fine, but standard DI just assigns it.
            _controller = new UsersController(
                _mockUserService.Object, 
                _mockAuthService.Object, 
                _mockPolicyService.Object, 
                null! 
            );
        }

        [Fact]
        public async Task CreateUser_ShouldReturnOk_WhenServiceSucceeds()
        {
            // Arrange
            var dto = new CreateUserDto 
            { 
                Email = "agent@test.com", 
                Password = "Password123", 
                Role = "Agent", 
                FirstName = "Agent", 
                LastName = "Smith" 
            };

            _mockAuthService.Setup(s => s.CreateStaffAsync(dto))
                .ReturnsAsync("Agent created successfully");

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            _mockAuthService.Verify(s => s.CreateStaffAsync(dto), Times.Once);
        }

        [Fact]
        public async Task CreateUser_ShouldReturnBadRequest_WhenServiceThrows()
        {
            // Arrange
            var dto = new CreateUserDto { Email = "duplicate@test.com" };
            
            _mockAuthService.Setup(s => s.CreateStaffAsync(dto))
                .ThrowsAsync(new Exception("User already exists"));

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("User already exists", badRequestResult.Value!.ToString());
        }
    }
}
