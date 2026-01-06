using Moq;
using Xunit;
using SmartHealthInsurance.Api.Controllers;
using SmartHealthInsurance.Api.Services;
using SmartHealthInsurance.Api.DTOs;
using SmartHealthInsurance.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace SmartHealthInsurance.Tests
{
    public class PoliciesControllerTests
    {
        private readonly Mock<IPolicyService> _mockPolicyService;
        private readonly PoliciesController _controller;

        public PoliciesControllerTests()
        {
            _mockPolicyService = new Mock<IPolicyService>();
            _controller = new PoliciesController(_mockPolicyService.Object);

            // 🔑 Mocking the User (ClaimsPrincipal)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new System.Security.Claims.Claim[]
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, "101"), // Agent ID
                new System.Security.Claims.Claim(ClaimTypes.Role, "InsuranceAgent")
            }, "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task CreatePolicy_ShouldReturnOk_WhenEnrollmentSucceeds()
        {
            // Arrange
            var dto = new PolicyCreateDto { PlanId = 1, Email = "test@client.com" };
            _mockPolicyService.Setup(s => s.EnrollPolicyAsync(dto, 101))
                .ReturnsAsync("Policy enrolled successfully");

            // Act
            var result = await _controller.CreatePolicy(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify structure of anonymous object { message = ... }
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetPolicyById_ShouldReturnNotFound_WhenPolicyDoesNotExist()
        {
            // Arrange
            int policyId = 999;
            _mockPolicyService.Setup(s => s.GetPolicyByIdAsync(policyId))
                .ReturnsAsync((Policy)null);

            // Act
            var result = await _controller.GetPolicyById(policyId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
