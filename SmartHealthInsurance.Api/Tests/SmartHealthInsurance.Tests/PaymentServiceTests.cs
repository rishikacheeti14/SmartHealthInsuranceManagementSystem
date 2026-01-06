using Moq;
using Xunit;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using SmartHealthInsurance.Api.Models;
using SmartHealthInsurance.Api.DTOs;
using System.Threading.Tasks;

namespace SmartHealthInsurance.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _mockPaymentRepo;
        private readonly Mock<IPolicyRepository> _mockPolicyRepo;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockPolicyRepo = new Mock<IPolicyRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _paymentService = new PaymentService(_mockPaymentRepo.Object, _mockPolicyRepo.Object, _mockNotificationService.Object);
        }

        [Fact]
        public async Task PayPremium_ShouldSucceed_WhenAmountIsCorrect()
        {
            // Arrange
            var policyId = 1;
            var policy = new Policy 
            { 
                PolicyId = policyId, 
                PremiumAmount = 1000m, 
                PremiumPaid = false 
            };

            var dto = new PremiumPaymentDto 
            { 
                PolicyId = policyId, 
                Amount = 1000m, 
                PaymentMethod = "CreditCard" 
            };

            _mockPolicyRepo.Setup(r => r.GetPolicyWithPlanAsync(policyId)).ReturnsAsync(policy);
            _mockPaymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockPolicyRepo.Setup(r => r.UpdateAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);

            // Act
            var result = await _paymentService.PayPremiumAsync(dto);

            // Assert
            Assert.Contains("Premium payment successful", result);
            Assert.True(policy.PremiumPaid); // Verify state change
            _mockPaymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
        }

        [Fact]
        public async Task PayPremium_ShouldThrow_WhenAmountIsInsufficient()
        {
            // Arrange
            var policyId = 1;
            var policy = new Policy 
            { 
                PolicyId = policyId, 
                PremiumAmount = 100.00m, 
                PremiumPaid = false 
            };

            var dto = new PremiumPaymentDto 
            { 
                PolicyId = policyId, 
                Amount = 50.00m // Insufficient
            };

            _mockPolicyRepo.Setup(r => r.GetPolicyWithPlanAsync(policyId)).ReturnsAsync(policy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<System.Exception>(() => _paymentService.PayPremiumAsync(dto));
            Assert.Contains("Insufficient payment amount", ex.Message);
        }
    }
}
