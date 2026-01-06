using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHealthInsurance.Api.Services;

namespace SmartHealthInsurance.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet("dashboard-stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var data = await _reportService.GetDashboardStatsAsync();
            return Ok(data);
        }

        [HttpGet("revenue-trend")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenueTrend()
        {
            var data = await _reportService.GetRevenueTrendAsync();
            return Ok(data);
        }
        [HttpGet("policies-distribution")]
        [Authorize(Roles = "InsuranceAgent,ClaimsOfficer")]
        public async Task<IActionResult> GetPolicyDistribution()
        {
            var data = await _reportService.GetPolicyDistributionAsync();
            return Ok(data);
        }
        [HttpGet("claims-analytics")]
        [Authorize(Roles = "InsuranceAgent,ClaimsOfficer")]
        public async Task<IActionResult> GetClaimsAnalytics()
        {
            var data = await _reportService.GetClaimsAnalyticsAsync();
            return Ok(data);
        }
        [HttpGet("premium-vs-payout")]
        [Authorize(Roles = "Customer,InsuranceAgent,ClaimsOfficer")]
        public async Task<IActionResult> GetPremiumVsPayout()
        {
            int? userId = null;
 
            var IsPrivileged = User.IsInRole("InsuranceAgent") || User.IsInRole("ClaimsOfficer") 
                               || User.HasClaim(c => c.Type == System.Security.Claims.ClaimTypes.Role && (c.Value == "InsuranceAgent" || c.Value == "ClaimsOfficer"))
                               || User.HasClaim(c => c.Type == "role" && (c.Value == "InsuranceAgent" || c.Value == "ClaimsOfficer"));

            if (!IsPrivileged)
            {
                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("nameid") 
                             ?? User.FindFirst("sub")
                             ?? User.FindFirst("UserId"); 
                
                if (idClaim != null && int.TryParse(idClaim.Value, out int parsedId))
                {
                    userId = parsedId;
                }
                else
                {
                    return Unauthorized("Unable to determine user identity for report scoping.");
                }
            }

            var data = await _reportService.GetPremiumVsPayoutAsync(userId);
            return Ok(data);
        }

        [HttpGet("high-value-claims")]
        [Authorize(Roles = "InsuranceAgent,ClaimsOfficer")]
        public async Task<IActionResult> GetHighValueClaims()
        {
            var data = await _reportService.GetHighValueClaimsAsync();
            return Ok(data);
        }
    }
}
