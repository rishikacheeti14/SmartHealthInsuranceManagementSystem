using Moq;
using Xunit;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.Repositories;
using SmartHealthInsurance.Api.DTOs;
using System.Threading.Tasks;

namespace SmartHealthInsurance.Tests
{
    public class ReportServiceTests
    {
        private readonly Mock<IPolicyRepository> _mockPolicyRepo;
        private readonly Mock<IClaimRepository> _mockClaimRepo;
        private readonly Mock<IPaymentRepository> _mockPaymentRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _mockPolicyRepo = new Mock<IPolicyRepository>();
            _mockClaimRepo = new Mock<IClaimRepository>();
            _mockPaymentRepo = new Mock<IPaymentRepository>();
            _mockUserRepo = new Mock<IUserRepository>();

            _service = new ReportService(
                _mockPolicyRepo.Object,
                _mockClaimRepo.Object,
                _mockPaymentRepo.Object,
                _mockUserRepo.Object
            );
        }

        [Fact]
        public async Task GetDashboardStats_ShouldReturnCounts()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.CountAsync(null)).ReturnsAsync(100);
            _mockPolicyRepo.Setup(r => r.CountAsync(null)).ReturnsAsync(50);
            _mockPaymentRepo.Setup(r => r.GetTotalPremiumsAsync(null)).ReturnsAsync(50000m);
            _mockClaimRepo.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<SmartHealthInsurance.Api.Models.Claim, bool>>>()))
                          .ReturnsAsync(5);

            // Act
            var result = await _service.GetDashboardStatsAsync();

            // Assert
            Assert.Equal(100, result.TotalUsers);
            Assert.Equal(50, result.TotalPolicies);
            Assert.Equal(50000m, result.TotalRevenue);
            Assert.Equal(5, result.ActiveClaims);
        }
    }
}
