using Moq;
using Xunit;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Helpers;
using Microsoft.AspNetCore.Identity;
using SmartHealthInsurance.Api.Enums;
using System.Threading.Tasks;
using System;

namespace SmartHealthInsurance.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockRepo;
        private readonly Mock<IPasswordHasher<User>> _mockHasher;
        private readonly Mock<IJwtTokenHelper> _mockTokenHelper;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockRepo = new Mock<IUserRepository>();
            _mockHasher = new Mock<IPasswordHasher<User>>();
            _mockTokenHelper = new Mock<IJwtTokenHelper>(); 

            _authService = new AuthService(_mockRepo.Object, _mockHasher.Object, _mockTokenHelper.Object);
        }

        [Fact]
        public async Task RegisterCustomer_ShouldReturnSuccess_WhenUserIsNew()
        {
            // Arrange
            var dto = new RegisterDto 
            { 
                Email = "test@example.com", 
                Password = "Password123", 
                FirstName = "John", 
                LastName = "Doe" 
            };

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null!);
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            _mockHasher.Setup(h => h.HashPassword(It.IsAny<User>(), dto.Password)).Returns("hashed_secret");

            // Act
            var result = await _authService.RegisterCustomerAsync(dto);

            // Assert
            Assert.Equal("Registered successfully", result);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterCustomer_ShouldThrowException_WhenUserExists()
        {
            // Arrange
            var dto = new RegisterDto { Email = "exist@example.com" };
            var existingUser = new User { Email = "exist@example.com", PasswordHash = "somehash" };

            _mockRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(existingUser);
            _mockHasher.Setup(h => h.VerifyHashedPassword(existingUser, existingUser.PasswordHash, "Welcome@123"))
                       .Returns(PasswordVerificationResult.Failed); // Simulating REAL existing user, not placeholder

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _authService.RegisterCustomerAsync(dto));
            Assert.Contains("User already exists", ex.Message);
        }
    }
}
