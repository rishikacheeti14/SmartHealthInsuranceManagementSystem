using Moq;
using Xunit;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using SmartHealthInsurance.Api.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;

namespace SmartHealthInsurance.Tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _mockRepo;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _mockRepo = new Mock<INotificationRepository>();
            _service = new NotificationService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateNotification_ShouldAddNotification()
        {
            // Arrange
            int userId = 1;
            string message = "Test Message";
            
            _mockRepo.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            // Act
            await _service.CreateNotificationAsync(userId, message);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(It.Is<Notification>(n => n.UserId == userId && n.Message == message && !n.IsRead)), Times.Once);
        }

        [Fact]
        public async Task GetMyNotifications_ShouldReturnList()
        {
            // Arrange
            int userId = 1;
            var list = new List<Notification> { new Notification { NotificationId = 1, UserId = userId, Message = "Hi" } };
            _mockRepo.Setup(r => r.GetAllByUserIdAsync(userId)).ReturnsAsync(list);

            // Act
            var result = await _service.GetMyNotificationsAsync(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Hi", result.First().Message);
        }

        [Fact]
        public async Task MarkAsRead_ShouldUpdate_WhenExists()
        {
            // Arrange
            int notifId = 1;
            var notification = new Notification { NotificationId = 1, IsRead = false };
            _mockRepo.Setup(r => r.GetByIdAsync(notifId)).ReturnsAsync(notification);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);

            // Act
            await _service.MarkAsReadAsync(notifId);

            // Assert
            Assert.True(notification.IsRead);
            _mockRepo.Verify(r => r.UpdateAsync(notification), Times.Once);
        }
    }
}
