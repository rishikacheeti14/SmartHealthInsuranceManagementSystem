using Moq;
using Xunit;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;

namespace SmartHealthInsurance.Tests
{
    public class TreatmentServiceTests
    {
        private readonly Mock<ITreatmentRepository> _mockTreatmentRepo;
        private readonly Mock<IHospitalRepository> _mockHospitalRepo;
        private readonly Mock<IPolicyRepository> _mockPolicyRepo;
        private readonly TreatmentService _treatmentService;

        public TreatmentServiceTests()
        {
            _mockTreatmentRepo = new Mock<ITreatmentRepository>();
            _mockHospitalRepo = new Mock<IHospitalRepository>();
            _mockPolicyRepo = new Mock<IPolicyRepository>();

            _treatmentService = new TreatmentService(
                _mockTreatmentRepo.Object,
                _mockHospitalRepo.Object,
                _mockPolicyRepo.Object
            );
        }

        [Fact]
        public async Task SubmitTreatment_ShouldSucceed_WhenPolicyIsActive()
        {
            // Arrange
            int providerUserId = 99;
            var dto = new TreatmentCreateDto 
            { 
                PolicyNumber = "POL-12345678",
                TreatmentCost = 5000 
            };

            var hospital = new Hospital { HospitalId = 1, UserId = providerUserId };
            var policy = new Policy 
            { 
                PolicyId = 10, 
                PolicyNumber = "POL-12345678", 
                Status = SmartHealthInsurance.Api.Enums.PolicyStatus.Active,
                UserId = 5
            };

            // Setup Hospital Repo to find by user
            _mockHospitalRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Hospital, bool>>>()))
                .ReturnsAsync(new List<Hospital> { hospital });

            // Setup Policy Repo to find by number
            _mockPolicyRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Policy, bool>>>()))
                .ReturnsAsync(new List<Policy> { policy });

            _mockTreatmentRepo.Setup(r => r.AddAsync(It.IsAny<TreatmentRecord>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _treatmentService.SubmitTreatmentAsync(dto, providerUserId);

            // Assert
            Assert.Contains("submitted successfully", result);
            _mockTreatmentRepo.Verify(r => r.AddAsync(It.IsAny<TreatmentRecord>()), Times.Once);
        }

        [Fact]
        public async Task SubmitTreatment_ShouldThrow_WhenPolicyIsNotActive()
        {
             // Arrange
            int providerUserId = 99;
            var dto = new TreatmentCreateDto { PolicyNumber = "POL-EXPIRED" };
            var hospital = new Hospital { HospitalId = 1, UserId = providerUserId };
            var policy = new Policy 
            { 
                PolicyId = 10, 
                PolicyNumber = "POL-EXPIRED", 
                Status = SmartHealthInsurance.Api.Enums.PolicyStatus.Expired
            };

            _mockHospitalRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Hospital, bool>>>()))
                .ReturnsAsync(new List<Hospital> { hospital });

            _mockPolicyRepo.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Policy, bool>>>()))
                .ReturnsAsync(new List<Policy> { policy });

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _treatmentService.SubmitTreatmentAsync(dto, providerUserId));
        }
    }
}
