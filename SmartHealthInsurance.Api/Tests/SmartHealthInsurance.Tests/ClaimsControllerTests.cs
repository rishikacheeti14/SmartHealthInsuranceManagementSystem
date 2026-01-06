using Moq;
using Xunit;
using SmartHealthInsurance.Api.Controllers;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartHealthInsurance.Tests
{
    public class ClaimsControllerTests
    {
        private readonly Mock<IClaimService> _mockClaimService;
        private readonly ClaimsController _controller;

        public ClaimsControllerTests()
        {
            _mockClaimService = new Mock<IClaimService>();
            _controller = new ClaimsController(_mockClaimService.Object);
        }

        private void SetupUser(string userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task ReviewClaim_ShouldReturnOk_WhenReviewIsSuccessful()
        {
            // Arrange
            int officerId = 99;
            int claimId = 1;
            SetupUser(officerId.ToString(), "ClaimsOfficer");

            var dto = new ClaimReviewDto { IsApproved = true, ApprovedAmount = 500 };
            
            _mockClaimService.Setup(s => s.ReviewClaimAsync(claimId, dto, officerId))
                .ReturnsAsync("Claim approved");

            // Act
            var result = await _controller.ReviewClaim(claimId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // We can also verify the value if needed
            // Assert.Equal("{ Message = Claim approved }", okResult.Value.ToString()); // Simplified check
            _mockClaimService.Verify(s => s.ReviewClaimAsync(claimId, dto, officerId), Times.Once);
        }
    }
}
