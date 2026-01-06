using Moq;
using Xunit;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Enums;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SmartHealthInsurance.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IPolicyRepository> _mockPolicyRepo;
        private readonly Mock<IClaimRepository> _mockClaimRepo;
        private readonly Mock<IHospitalRepository> _mockHospitalRepo;
        private readonly Mock<INotificationRepository> _mockNotifyRepo;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockPolicyRepo = new Mock<IPolicyRepository>();
            _mockClaimRepo = new Mock<IClaimRepository>();
            _mockHospitalRepo = new Mock<IHospitalRepository>();
            _mockNotifyRepo = new Mock<INotificationRepository>();

            _service = new UserService(
                _mockUserRepo.Object,
                _mockPolicyRepo.Object,
                _mockClaimRepo.Object,
                _mockHospitalRepo.Object,
                _mockNotifyRepo.Object
            );
        }

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var user = new User { UserId = 1, Email = "test@test.com" };
            _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _service.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task UpdateUserRole_ShouldUpdate_WhenValid()
        {
            // Arrange
            var user = new User { UserId = 1, Role = UserRole.Customer };
            _mockUserRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateUserRoleAsync(1, new UpdateUserRoleDto { Role = "Admin" });

            // Assert
            Assert.Equal(UserRole.Admin, user.Role);
            _mockUserRepo.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldSuccess_WhenNoDependencies()
        {
            // Arrange
            int userId = 10;
            var user = new User { UserId = userId, Role = UserRole.Customer, Email = "cust@test.com" };
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            
            // Validation Mocks
            _mockPolicyRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Policy, bool>>>())).ReturnsAsync(false);
            _mockClaimRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Claim, bool>>>())).ReturnsAsync(false);
            _mockHospitalRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Hospital, bool>>>())).ReturnsAsync(false);
            _mockNotifyRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Notification, bool>>>())).ReturnsAsync(new List<Notification>());
            
            _mockNotifyRepo.Setup(r => r.DeleteAsync(It.IsAny<int>())).Returns(Task.CompletedTask);
            _mockUserRepo.Setup(r => r.DeleteAsync(userId)).Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteUserAsync(userId);

            // Assert
            Assert.Equal("User deleted successfully", result);
            _mockUserRepo.Verify(r => r.DeleteAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldThrow_WhenHasPolicies()
        {
            // Arrange
            int userId = 10;
            var user = new User { UserId = userId, Role = UserRole.Customer };
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            _mockPolicyRepo.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Policy, bool>>>())).ReturnsAsync(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.DeleteUserAsync(userId));
            Assert.Contains("Cannot delete user because they have associated policies", ex.Message);
        }
    }
}
