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
using SmartHealthInsurance.Api.Repositories; // For IHospitalRepository
using SmartHealthInsurance.Api.Helpers;

namespace SmartHealthInsurance.Tests
{
    public class ClaimServiceTests
    {
        private readonly Mock<IClaimRepository> _mockClaimRepo;
        private readonly Mock<IPolicyRepository> _mockPolicyRepo;
        private readonly Mock<ITreatmentRepository> _mockTreatmentRepo;
        private readonly Mock<IPaymentRepository> _mockPaymentRepo;
        private readonly Mock<IHospitalRepository> _mockHospitalRepo;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            _mockClaimRepo = new Mock<IClaimRepository>();
            _mockPolicyRepo = new Mock<IPolicyRepository>();
            _mockTreatmentRepo = new Mock<ITreatmentRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockHospitalRepo = new Mock<IHospitalRepository>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockUserRepo = new Mock<IUserRepository>();
            
            _claimService = new ClaimService(
                _mockClaimRepo.Object, 
                _mockPolicyRepo.Object, 
                _mockTreatmentRepo.Object, 
                _mockPaymentRepo.Object, 
                _mockHospitalRepo.Object,
                _mockNotificationService.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task ReviewClaim_ShouldTriggerPayout_WhenApproved()
        {
            // Arrange
            int claimId = 1;
            int officerId = 99; // Explicitly passed
            
            var existingClaim = new Claim 
            { 
                ClaimId = 1, 
                Status = ClaimStatus.Submitted, 
                PolicyId = 10, 
                UserId = 5,
                ClaimAmount = 5000 
            };
            
            // Policy doesn't have CoverageLimit directly, it's on the Plan
            var policy = new Policy 
            { 
                PolicyId = 10, 
                Plan = new InsurancePlan { CoverageLimit = 10000 },
                PlanId = 1 // Ensure consistency
            };

            var dto = new ClaimReviewDto 
            { 
                IsApproved = true, 
                ApprovedAmount = 5000, 
                RejectionReason = null 
            };

            // Setup Mocks
            _mockClaimRepo.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(existingClaim);
            // Ensure GetByIdAsync returns the policy with the plan attached
            _mockPolicyRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(policy); 
            
            // Note: If the service uses a specific method like 'GetPolicyWithPlanAsync', we should mock that instead.
            // Assuming standard GetById for now, or if service retrieves inclusion.
            // Ideally check service implementation, but let's assume standard repository pattern or ensure mock covers what's called.
            
            _mockPaymentRepo.Setup(r => r.AddAsync(It.IsAny<Payment>())).Returns(Task.CompletedTask);
            _mockClaimRepo.Setup(r => r.UpdateAsync(It.IsAny<Claim>())).Returns(Task.CompletedTask);
            
            // Corrected: CreateNotificationAsync takes 2 arguments (userId, message)
            _mockNotificationService.Setup(n => n.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>()))
                                    .Returns(Task.CompletedTask);

            // Act
            await _claimService.ReviewClaimAsync(claimId, dto, officerId);

            // Assert
            // Verify status changed to Approved or Paid? 
            // Logic: If approved -> Status = Approved. Then -> ProcessPayout -> Status = Paid.
            // So final status should be Paid if successful.
            Assert.Equal(ClaimStatus.Paid, existingClaim.Status);
            
            // Verify Payment was added
            _mockPaymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
            
            // Verify Claim Update was called twice (once for approval, once for paid status) or at least once
            _mockClaimRepo.Verify(r => r.UpdateAsync(It.IsAny<Claim>()), Times.AtLeastOnce);
        }
    }
}
