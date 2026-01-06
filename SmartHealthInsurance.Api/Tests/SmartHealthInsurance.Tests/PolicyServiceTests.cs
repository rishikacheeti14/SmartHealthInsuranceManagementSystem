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
using System.Linq;

namespace SmartHealthInsurance.Tests
{
    public class PolicyServiceTests
    {
        private readonly Mock<IPolicyRepository> _mockPolicyRepo;
        private readonly Mock<IRepository<InsurancePlan>> _mockPlanRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IClaimRepository> _mockClaimRepo;
        private readonly Mock<IPaymentRepository> _mockPaymentRepo;
        private readonly Mock<ITreatmentRepository> _mockTreatmentRepo;
        private readonly Mock<INotificationService> _mockNotificationService;

        private readonly PolicyService _policyService;

        public PolicyServiceTests()
        {
            _mockPolicyRepo = new Mock<IPolicyRepository>();
            _mockPlanRepo = new Mock<IRepository<InsurancePlan>>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockClaimRepo = new Mock<IClaimRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockTreatmentRepo = new Mock<ITreatmentRepository>();
            _mockNotificationService = new Mock<INotificationService>();

            _policyService = new PolicyService(
                _mockPolicyRepo.Object, 
                _mockPlanRepo.Object, 
                _mockUserRepo.Object,
                _mockClaimRepo.Object,
                _mockPaymentRepo.Object,
                _mockTreatmentRepo.Object,
                _mockNotificationService.Object
            );
        }

        [Fact]
        public async Task EnrollPolicy_ShouldCreatePolicy_WhenValid()
        {
            // Arrange
            var dto = new PolicyCreateDto 
            { 
                PlanId = 1, 
                Email = "new@customer.com", 
                FirstName = "New", 
                LastName = "User",
                StartDate = DateTime.UtcNow
            };
            var plan = new InsurancePlan { PlanId = 1, DurationInMonths = 12 };

            _mockPlanRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(plan);
            
            // Service requires user to exist
            var existingUser = new User { UserId = 55, Email = dto.Email, Role = UserRole.Customer, FirstName = "New", LastName = "User" };
            _mockUserRepo.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(existingUser);
            
            // _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>())).Returns(Task.CompletedTask); // No longer called
            
            _mockPolicyRepo.Setup(r => r.GetActivePolicyByUserIdAsync(existingUser.UserId)).ReturnsAsync((Policy)null!); // Ensure no duplicate policy
            _mockPolicyRepo.Setup(r => r.AddAsync(It.IsAny<Policy>())).Returns(Task.CompletedTask);
            _mockNotificationService.Setup(n => n.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            // Act
            var result = await _policyService.EnrollPolicyAsync(dto, agentId: 101);

            // Assert
            Assert.Contains("Policy enrolled successfully", result);
            _mockPolicyRepo.Verify(r => r.AddAsync(It.Is<Policy>(p => p.PremiumPaid == false)), Times.Once);
        }
    }
}
